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
		var args = job.GenerateArgsInternal();

		args.Inputs.Should().ContainSingle();
		args.Inputs[0].File.Should().Be(job.Anime.GetAbsoluteSourcePath());
		args.Inputs[0].Args.Should().HaveCount(2);
		args.GetValues("ss").Should().ContainSingle()
			.And.Contain(job.Song.Start.ToString());
		args.GetValues("to").Should().ContainSingle()
			.And.Contain(job.Song.End.ToString());
		args.Mapping.Should().HaveCount(2)
			.And.Contain(new[] { "0:v:0", "0:a:0" });
		args.AudioFilters.Should().BeNull();
		args.VideoFilters.Should().BeNull();
	}

	[TestMethod]
	public void ArgsVideoCleanPath_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.CleanPath = @"C:\joemama.wav";
			song.OverrideAudioTrack = 73;
			song.OverrideVideoTrack = 75;
		});
		var args = job.GenerateArgsInternal();

		args.Inputs.Should().HaveCount(2);
		args.Inputs[0].File.Should().Be(job.Anime.GetAbsoluteSourcePath());
		args.Inputs[0].Args.Should().HaveCount(2);
		args.Inputs[1].File.Should().Be(job.Song.CleanPath);
		args.Inputs[1].Args.Should().BeNull();
		args.GetValues("ss").Should().ContainSingle()
			.And.Contain(job.Song.Start.ToString());
		args.GetValues("to").Should().ContainSingle()
			.And.Contain(job.Song.End.ToString());
		args.Mapping.Should().HaveCount(2)
			.And.Contain(new[]
			{
				$"0:v:{job.Song.OverrideVideoTrack}",
				$"1:a:{job.Song.OverrideAudioTrack}",
			});
		args.AudioFilters.Should().BeNull();
		args.VideoFilters.Should().BeNull();
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
	public void ArgsVideoOverrideTracks_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.OverrideAudioTrack = 73;
			song.OverrideVideoTrack = 75;
		});
		var args = job.GenerateArgsInternal();

		args.Inputs.Should().ContainSingle();
		args.Inputs[0].File.Should().Be(job.Anime.GetAbsoluteSourcePath());
		args.Inputs[0].Args.Should().HaveCount(2);
		args.GetValues("ss").Should().ContainSingle()
			.And.Contain(job.Song.Start.ToString());
		args.GetValues("to").Should().ContainSingle()
			.And.Contain(job.Song.End.ToString());
		args.Mapping.Should().HaveCount(2)
			.And.Contain(new[]
			{
				$"0:v:{job.Song.OverrideVideoTrack}",
				$"0:a:{job.Song.OverrideAudioTrack}",
			});
		args.AudioFilters.Should().BeNull();
		args.VideoFilters.Should().BeNull();
	}

	[TestMethod]
	public void ArgsVideoVolumeModifier_Test()
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
		args.Mapping.Should().HaveCount(2)
			.And.Contain(new[] { "0:v:0", "0:a:0" });
		args.AudioFilters.Should().ContainSingle();
		args.GetValues("volume").Should().ContainSingle()
			.And.Contain(job.Song.VolumeModifier.ToString());
		args.VideoFilters.Should().BeNull();
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
		var job = GenerateJob(temp.Dir, (_, song) =>
		{
			song.VolumeModifier = VolumeModifer.FromDecibels(-5);
		});

		var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
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
}