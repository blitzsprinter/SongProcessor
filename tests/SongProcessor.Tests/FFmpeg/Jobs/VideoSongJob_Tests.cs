using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg;
using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Results;

namespace SongProcessor.Tests.FFmpeg.Jobs;

[TestClass]
public sealed class VideoSongJob_Tests : SongJob_TestsBase
{
	[TestMethod]
	public async Task Process_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			return new VideoSongJob(anime, song, ValidVideoInfo.Height);
		});

		var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var newVideoInfo = await Gatherer.GetVideoInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVideoInfo.Info);

		job.AlreadyExists.Should().BeTrue();
		var result = await job.ProcessAsync().ConfigureAwait(false);
		result.IsSuccess.Should().BeFalse();
		result.Should().BeOfType<FileAlreadyExists>();
	}

	[TestMethod]
	public async Task ProcessCanceled_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			return new VideoSongJob(anime, song, ValidVideoInfo.Height);
		});
		var cts = new CancellationTokenSource();

		var task = job.ProcessAsync(cts.Token);
		cts.Cancel();

		var result = await task.ConfigureAwait(false);
		result.IsSuccess.Should().BeNull();
		result.Should().BeOfType<Canceled>();
	}

	[TestMethod]
	public async Task ProcessModifyVolume_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			song.VolumeModifier = VolumeModifer.FromDecibels(-5);
			return new VideoSongJob(anime, song, ValidVideoInfo.Height);
		});

		var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
		newVolumeInfo.Info.MaxVolume.Should().BeLessThan(ValidVideoVolume.MaxVolume);
		newVolumeInfo.Info.MeanVolume.Should().BeLessThan(ValidVideoVolume.MeanVolume);

		var newVideoInfo = await Gatherer.GetVideoInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVideoInfo.Info);
	}

	[TestMethod]
	public async Task ProcessNullDAR_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			anime = new Anime(anime.AbsoluteInfoPath, anime, new(
				anime.VideoInfo!.Value.Path,
				anime.VideoInfo.Value.Info with
				{
					DAR = null
				}
			));
			return new VideoSongJob(anime, song, ValidVideoInfo.Height + 2);
		});

		Func<Task> process = () => job.ProcessAsync();
		await process.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false);
	}

	private static void AssertValidLength(SongJob job, VideoInfo info)
	{
		var duration = double.Parse(info.Tags["DURATION"].Split(':')[^1]);
		var expected = job.Song.GetLength().TotalSeconds;
		AssertValidLength(duration, expected);
	}
}