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
		var args = job.GenerateArgsInternal();

		args.Inputs.Should().ContainSingle();
		args.Inputs[0].File.Should().Be(job.Anime.GetAbsoluteSourcePath());
		args.Inputs[0].Args.Should().HaveCount(2);
		args.GetValues("ss").Should().ContainSingle()
			.And.Contain(job.Song.Start.ToString());
		args.GetValues("to").Should().ContainSingle()
			.And.Contain(job.Song.End.ToString());
		args.Mapping.Should().ContainSingle()
			.And.Contain("0:a:0");
		args.AudioFilters.Should().BeNull();
		args.VideoFilters.Should().BeNull();
	}

	[TestMethod]
	public void ArgsMp3CleanPath_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.CleanPath = @"C:\joemama.wav";
			song.OverrideAudioTrack = 73;
		});
		var args = job.GenerateArgsInternal();

		args.Inputs.Should().ContainSingle();
		args.Inputs[0].File.Should().Be(job.Song.CleanPath);
		args.Inputs[0].Args.Should().ContainSingle();
		args.GetValues("ss").Should().BeEmpty();
		args.GetValues("to").Should().ContainSingle()
			.And.Contain(job.Song.GetLength().ToString());
		args.Mapping.Should().ContainSingle()
			.And.Contain($"0:a:{job.Song.OverrideAudioTrack}");
		args.AudioFilters.Should().BeNull();
		args.VideoFilters.Should().BeNull();
	}

	[TestMethod]
	public void ArgsMp3OverrideAudioTrack_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.OverrideAudioTrack = 73;
		});
		var args = job.GenerateArgsInternal();

		args.Inputs.Should().ContainSingle();
		args.Inputs[0].File.Should().Be(job.Anime.GetAbsoluteSourcePath());
		args.Inputs[0].Args.Should().HaveCount(2);
		args.GetValues("ss").Should().ContainSingle()
			.And.Contain(job.Song.Start.ToString());
		args.GetValues("to").Should().ContainSingle()
			.And.Contain(job.Song.End.ToString());
		args.Mapping.Should().ContainSingle()
			.And.Contain($"0:a:{job.Song.OverrideAudioTrack}");
		args.AudioFilters.Should().BeNull();
		args.VideoFilters.Should().BeNull();
	}

	[TestMethod]
	public void ArgsMp3VolumeModifier_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.VolumeModifier = VolumeModifer.FromDecibels(-2);
		});
		var args = job.GenerateArgsInternal();

		args.Inputs.Should().ContainSingle();
		args.Inputs[0].File.Should().Be(job.Anime.GetAbsoluteSourcePath());
		args.Inputs[0].Args.Should().HaveCount(2);
		args.GetValues("ss").Should().ContainSingle()
			.And.Contain(job.Song.Start.ToString());
		args.GetValues("to").Should().ContainSingle()
			.And.Contain(job.Song.End.ToString());
		args.Mapping.Should().ContainSingle()
			.And.Contain("0:a:0");
		args.AudioFilters.Should().ContainSingle();
		args.GetValues("volume").Should().ContainSingle()
			.And.Contain(job.Song.VolumeModifier.ToString());
		args.VideoFilters.Should().BeNull();
	}

	[TestMethod]
	[TestCategory(FFMPEG_CATEGORY)]
	public async Task ProcessMp3_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir);

		var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVolumeInfo.Info);

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
			song.End = TimeSpan.FromSeconds(anime.VideoInfo!.Value.Info.Duration!.Value);
		});

		// Create a duplicate version to treat as a clean version
		var cleanPath = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var cleanVolumeInfo = await Gatherer.GetVolumeInfoAsync(cleanPath).ConfigureAwait(false);
		AssertValidLength(cleanVolumeInfo.Info.NSamples, ValidVideoVolume.NSamples);

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
		AssertValidLength(job, newVolumeInfo.Info);
		newVolumeInfo.Info.MaxVolume.Should().BeLessThan(ValidVideoVolume.MaxVolume);
		newVolumeInfo.Info.MeanVolume.Should().BeLessThan(ValidVideoVolume.MeanVolume);
	}

	protected override Mp3SongJob GenerateJob(Anime anime, Song song)
		=> new(anime, song);

	private void AssertValidLength(SongJob job, VolumeInfo info)
	{
		var duration = (double)info.NSamples;
		var divisor = ValidVideoInfo.Duration / job.Song.GetLength().TotalSeconds;
		var expected = (ValidVideoVolume.NSamples / divisor)!.Value;
		AssertValidLength(duration, expected);
	}
}