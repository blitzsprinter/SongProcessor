using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Results;

namespace SongProcessor.Tests.FFmpeg.Jobs
{
	[TestClass]
	public sealed class VideoSongJob_Tests : SongJob_TestsBase
	{
		[TestMethod]
		public async Task Process_Test()
		{
			using var temp = new TempDirectory();

			var (anime, song) = GenerateAnimeAndSong(temp.Dir);
			var job = new VideoSongJob(anime, song, ValidVideoInfo.Height);
			var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);

			var newVideoInfo = await Gatherer.GetVideoInfoAsync(file).ConfigureAwait(false);
			// Returns '00:00:01.342000000'
			var duration = double.Parse(newVideoInfo.Info.Tags["DURATION"].Split(':')[^1]);
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

			var (anime, song) = GenerateAnimeAndSong(temp.Dir);
			var job = new VideoSongJob(anime, song, ValidVideoInfo.Height);
			var source = new CancellationTokenSource();

			var task = job.ProcessAsync(source.Token);
			source.Cancel();

			var result = await task.ConfigureAwait(false);
			Assert.IsNull(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(Canceled));
		}

		[TestMethod]
		public async Task ProcessModifyVolume_Test()
		{
			using var temp = new TempDirectory();

			var (anime, song) = GenerateAnimeAndSong(temp.Dir);
			song.VolumeModifier = VolumeModifer.FromDecibels(-5);
			var job = new VideoSongJob(anime, song, ValidVideoInfo.Height);
			var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);

			var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
			Assert.IsTrue(ValidVideoVolume.MaxVolume > newVolumeInfo.Info.MaxVolume);
			Assert.IsTrue(ValidVideoVolume.MeanVolume > newVolumeInfo.Info.MeanVolume);

			var newVideoInfo = await Gatherer.GetVideoInfoAsync(file).ConfigureAwait(false);
			// Returns '00:00:01.342000000'
			var duration = double.Parse(newVideoInfo.Info.Tags["DURATION"].Split(':')[^1]);
			var expected = ValidVideoInfo.Duration!.Value / DIV;
			Assert.AreEqual(expected, duration, expected * 0.05);
		}

		[TestMethod]
		public async Task ProcessNullDAR_Test()
		{
			using var temp = new TempDirectory();

			var (anime, song) = GenerateAnimeAndSong(temp.Dir);
			anime = new Anime(anime.AbsoluteInfoPath, anime, new(
				anime.VideoInfo!.Value.Path,
				anime.VideoInfo.Value.Info with
				{
					DAR = null
				}
			));
			var job = new VideoSongJob(anime, song, ValidVideoInfo.Height + 2);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
			{
				await job.ProcessAsync().ConfigureAwait(false);
			}).ConfigureAwait(false);
		}
	}
}