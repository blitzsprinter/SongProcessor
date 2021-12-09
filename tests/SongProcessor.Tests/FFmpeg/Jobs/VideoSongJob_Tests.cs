using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Results;

namespace SongProcessor.Tests.FFmpeg.Jobs;

[TestClass]
public sealed class VideoSongJob_Tests : SongJob_TestsBase
{
	public const double EXPECTED_DURATION = 1.342d;

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
		var duration = double.Parse(newVideoInfo.Info.Tags["DURATION"].Split(':')[^1]);
		Assert.AreEqual(EXPECTED_DURATION, duration, EXPECTED_DURATION * 0.05);
		var expected = ValidVideoInfo.Duration!.Value / DIV;
		Assert.AreEqual(expected, duration, expected * 0.05);

		Assert.IsTrue(job.AlreadyExists);
		var result = await job.ProcessAsync().ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
		Assert.IsInstanceOfType(result, typeof(FileAlreadyExists));
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
		Assert.IsNull(result.IsSuccess);
		Assert.IsInstanceOfType(result, typeof(Canceled));
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
		Assert.IsTrue(ValidVideoVolume.MaxVolume > newVolumeInfo.Info.MaxVolume);
		Assert.IsTrue(ValidVideoVolume.MeanVolume > newVolumeInfo.Info.MeanVolume);

		var newVideoInfo = await Gatherer.GetVideoInfoAsync(file).ConfigureAwait(false);
		var duration = double.Parse(newVideoInfo.Info.Tags["DURATION"].Split(':')[^1]);
		Assert.AreEqual(EXPECTED_DURATION, duration, EXPECTED_DURATION * 0.05);
		var expected = ValidVideoInfo.Duration!.Value / DIV;
		Assert.AreEqual(expected, duration, expected * 0.05);
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

		await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
		{
			await job.ProcessAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);
	}
}