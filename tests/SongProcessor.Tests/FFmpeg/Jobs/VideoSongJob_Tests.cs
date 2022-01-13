using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg;
using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Results;

namespace SongProcessor.Tests.FFmpeg.Jobs;

[TestClass]
public sealed class VideoSongJob_Tests : SongJob_TestsBase<VideoSongJob>
{
	[TestMethod]
	public void ArgsVideo_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir);
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().BeEquivalentTo(@default);
	}

	[TestMethod]
	public void ArgsVideoCleanPath_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.CleanPath = @"C:\joemama.wav";
		});
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().NotBeEquivalentTo(@default);
		actual.Should().BeEquivalentTo(@default with
		{
			Inputs = new JobInput[]
			{
				@default.Inputs[0],
				new(job.Song.GetCleanPath(job.Anime)!, null!),
			},
			Mapping = new[] { "0:v:0", "1:a:0" },
		});
	}

	[TestMethod]
	public void ArgsVideoHeightUnequalToResolution_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir);
		job = new(job.Anime, job.Song, job.Resolution + 2);
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().NotBeEquivalentTo(@default);
		actual.Should().BeEquivalentTo(@default with
		{
			VideoFilters = new Dictionary<string, string>
			{
				["setsar"] = AspectRatio.Square.ToString(),
				["setdar"] = job.Anime.VideoInfo?.Info.DAR?.ToString()!,
				["scale"] = "484:272",
			},
		});
	}

	[TestMethod]
	public void ArgsVideoNonSquareSAR_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, configureAnime: anime =>
		{
			return new Anime(anime.AbsoluteInfoPath, anime, new(
				anime.VideoInfo!.Value.Path,
				anime.VideoInfo.Value.Info with
				{
					SAR = new(4, 3),
				}
			));
		});
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().NotBeEquivalentTo(@default);
		actual.Should().BeEquivalentTo(@default with
		{
			VideoFilters = new Dictionary<string, string>
			{
				["setsar"] = AspectRatio.Square.ToString(),
				["setdar"] = job.Anime.VideoInfo?.Info.DAR?.ToString()!,
				["scale"] = "480:270",
			},
		});
	}

	[TestMethod]
	public void ArgsVideoNullDAR_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, configureAnime: anime =>
		{
			return new Anime(anime.AbsoluteInfoPath, anime, new(
				anime.VideoInfo!.Value.Path,
				anime.VideoInfo.Value.Info with
				{
					DAR = null
				}
			));
		});
		job = new(job.Anime, job.Song, job.Resolution + 2);

		Action args = () => job.GenerateArgsInternal();
		args.Should().Throw<InvalidOperationException>();
	}

	[TestMethod]
	public void ArgsVideoOverrideAspectRatio_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.OverrideAspectRatio = new AspectRatio(4, 3);
		});
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().NotBeEquivalentTo(@default);
		actual.Should().BeEquivalentTo(@default with
		{
			VideoFilters = new Dictionary<string, string>
			{
				["setsar"] = AspectRatio.Square.ToString(),
				["setdar"] = job.Song.OverrideAspectRatio?.ToString()!,
				["scale"] = "360:270",
			},
		});
	}

	[TestMethod]
	public void ArgsVideoOverrideTracks_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.OverrideAudioTrack = 73;
			song.OverrideVideoTrack = 75;
		});
		var actual = job.GenerateArgsInternal();

		var @default = GenerateDefaultJobArgs(job);
		actual.Should().NotBeEquivalentTo(@default);
		actual.Should().BeEquivalentTo(@default with
		{
			Mapping = new[]
			{
				$"0:a:{job.Song.OverrideAudioTrack}",
				$"0:v:{job.Song.OverrideVideoTrack}",
			},
		});
	}

	[TestMethod]
	public void ArgsVideoVolumeModifier_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.VolumeModifier = VolumeModifer.FromDecibels(-2);
		});
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
	public async Task ProcessVideo_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir);

		var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var newVideoInfo = await Gatherer.GetVideoInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVideoInfo.Info);

		job.AlreadyExists.Should().BeTrue();
		var result = await job.ProcessAsync().ConfigureAwait(false);
		result.IsSuccess.Should().BeFalse();
		result.Should().BeOfType<FileAlreadyExists>();
	}

	[TestMethod]
	[TestCategory(FFMPEG_CATEGORY)]
	public async Task ProcessVideoCanceled_Test()
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
	public async Task ProcessVideoComplicated_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			// Generate a clean version from the entire video
			song.End = TimeSpan.FromSeconds(anime.VideoInfo!.Value.Info.Duration!.Value);
		});

		// Create a duplicate version to treat as a clean version
		var cleanPath = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var cleanVideoInfo = await Gatherer.GetVideoInfoAsync(cleanPath).ConfigureAwait(false);
		AssertValidLength(job, cleanVideoInfo.Info);

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
		newVolumeInfo.Info.MaxVolume.Should().BeLessThan(ValidVideoVolume.MaxVolume);
		newVolumeInfo.Info.MeanVolume.Should().BeLessThan(ValidVideoVolume.MeanVolume);

		var newVideoInfo = await Gatherer.GetVideoInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVideoInfo.Info);
	}

	protected override VideoSongJob GenerateJob(Anime anime, Song song)
		=> new(anime, song, ValidVideoInfo.Height);

	private static void AssertValidLength(SongJob job, VideoInfo info)
	{
		var duration = double.Parse(info.Tags["DURATION"].Split(':')[^1]);
		var expected = job.Song.GetLength().TotalSeconds;
		AssertValidLength(duration, expected);
	}

	private static JobArgs GenerateDefaultJobArgs(VideoSongJob job)
	{
		return new JobArgs(
			Inputs: new JobInput[]
			{
				new(job.Anime.GetAbsoluteSourcePath(), new Dictionary<string, string>
				{
					["ss"] = job.Song.Start.ToString(),
					["to"] = job.Song.End.ToString(),
				}),
			},
			Mapping: new[] { "0:v:0", "0:a:0" },
			QualityArgs: VideoSongJob.VideoArgs,
			AudioFilters: null,
			VideoFilters: null,
			OutputFile: job.Song.GetVideoPath(job.Anime, job.Resolution)
		);
	}
}