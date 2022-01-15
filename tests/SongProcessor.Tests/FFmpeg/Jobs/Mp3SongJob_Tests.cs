using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg;
using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Results;

namespace SongProcessor.Tests.FFmpeg.Jobs;

[TestClass]
public sealed class Mp3SongJob_Tests : SongJob_TestsBase<Mp3SongJob>
{
	[TestMethod]
	public void ArgsMp3_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir);
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().BeEquivalentTo(@default);
	}

	[TestMethod]
	public void ArgsMp3CleanPath_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) => song.CleanPath = @"C:\joemama.wav");
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().NotBeEquivalentTo(@default);
		actual.Should().BeEquivalentTo(@default with
		{
			Inputs = new FFmpegInput[]
			{
				new(job.Song.GetCleanFile(job.Anime)!, new Dictionary<string, string>
				{
					["to"] = job.Song.GetLength().ToString(),
				}),
			},
		});
	}

	[TestMethod]
	public void ArgsMp3OverrideAudioTrack_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) => song.OverrideAudioTrack = 73);
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().NotBeEquivalentTo(@default);
		actual.Should().BeEquivalentTo(@default with
		{
			Mapping = new[]
			{
				$"0:a:{job.Song.OverrideAudioTrack}",
			},
		});
	}

	[TestMethod]
	public void ArgsMp3VolumeModifier_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir,
			(_, song) => song.VolumeModifier = VolumeModifer.FromDecibels(-2));
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().NotBeEquivalentTo(@default);
		actual.Should().BeEquivalentTo(@default with
		{
			AudioFilters = new Dictionary<string, string>
			{
				["volume"] = job.Song.VolumeModifier.ToString()!,
			},
		});
	}

	[TestMethod]
	[TestCategory(FFMPEG_CATEGORY)]
	public async Task ProcessMp3_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir);

		var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVolumeInfo);

		job.AlreadyExists.Should().BeTrue();
		var result = await job.ProcessAsync().ConfigureAwait(false);
		result.IsSuccess.Should().BeFalse();
		result.Should().BeOfType<FileAlreadyExists>();
	}

	[TestMethod]
	[TestCategory(FFMPEG_CATEGORY)]
	public async Task ProcessMp3Canceled_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir);
		var cts = new CancellationTokenSource();

		var task = job.ProcessAsync(cts.Token);
		cts.Cancel();

		var result = await task.ConfigureAwait(false);
		result.IsSuccess.Should().BeNull();
		result.Should().BeOfType<Canceled>();
	}

	[TestMethod]
	[TestCategory(FFMPEG_CATEGORY)]
	public async Task ProcessMp3Complicated_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			// Generate a clean version from the entire video
			song.End = TimeSpan.FromSeconds(anime.VideoInfo!.Duration!.Value);
		});

		// Create a duplicate version to treat as a clean version
		var cleanPath = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var cleanVolumeInfo = await Gatherer.GetVolumeInfoAsync(cleanPath).ConfigureAwait(false);
		AssertValidLength(cleanVolumeInfo.NSamples, VolumeInfo.NSamples);

		{
			var movedPath = Path.Combine(
				Path.GetDirectoryName(cleanPath)!,
				$"clean{Path.GetExtension(cleanPath)}");
			File.Move(cleanPath, movedPath);
			cleanPath = movedPath;
		}

		job = GenerateJob(temp.Dir, (_, song) =>
		{
			// Use the newly created clean path
			song.CleanPath = cleanPath;
			song.VolumeModifier = VolumeModifer.FromDecibels(-5);
		});

		var result = await job.ProcessAsync().ConfigureAwait(false);
		result.IsSuccess.Should().BeTrue();
		// Delete the clean version so we can get the output easier
		File.Delete(cleanPath);

		var file = GetSingleFile(temp.Dir);
		var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVolumeInfo);
		newVolumeInfo.MaxVolume.Should().BeLessThan(VolumeInfo.MaxVolume);
		newVolumeInfo.MeanVolume.Should().BeLessThan(VolumeInfo.MeanVolume);
	}

	protected override Mp3SongJob GenerateJob(Anime anime, Song song)
		=> new(anime, song);

	private static FFmpegArgs GenerateDefaultJobArgs(Mp3SongJob job)
	{
		return new FFmpegArgs(
			Inputs: new FFmpegInput[]
			{
				new(job.Anime.GetSourceFile(), new Dictionary<string, string>
				{
					["ss"] = job.Song.Start.ToString(),
					["to"] = job.Song.End.ToString(),
				}),
			},
			Mapping: new[] { "0:a:0" },
			Args: Mp3SongJob.AudioArgs,
			AudioFilters: null,
			VideoFilters: null,
			OutputFile: job.Song.GetMp3File(job.Anime)
		);
	}

	private void AssertValidLength(SongJob job, VolumeInfo info)
	{
		var duration = (double)info.NSamples;
		var divisor = VideoInfo.Duration!.Value / job.Song.GetLength().TotalSeconds;
		var expected = VolumeInfo.NSamples / divisor;
		AssertValidLength(duration, expected);
	}
}