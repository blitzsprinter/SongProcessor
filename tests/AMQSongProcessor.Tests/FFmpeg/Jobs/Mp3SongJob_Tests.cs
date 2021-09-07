using AMQSongProcessor.FFmpeg.Jobs;
using AMQSongProcessor.Models;
using AMQSongProcessor.Results;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.FFmpeg.Jobs
{
	[TestClass]
	[TestCategory(CATEGORY)]
	public sealed class Mp3SongJob_Tests : FFmpeg_TestsBase
	{
		private const int DIV = 4;

		[TestMethod]
		public async Task Process_Test()
		{
			using var temp = new TempDirectory();

			var (anime, song) = GenerateAnimeAndSong(temp.Dir);
			var job = new Mp3SongJob(anime, song);
			var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);

			var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
			Assert.IsTrue(ValidVideoVolume.NSamples / DIV >= newVolumeInfo.Info.NSamples);

			Assert.IsTrue(job.AlreadyExists);
			var result = await job.ProcessAsync().ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(FileAlreadyExistsResult));
		}

		[TestMethod]
		public async Task ProcessCanceled_Test()
		{
			using var temp = new TempDirectory();

			var (anime, song) = GenerateAnimeAndSong(temp.Dir);
			var job = new Mp3SongJob(anime, song);
			var source = new CancellationTokenSource();

			var task = job.ProcessAsync(source.Token);
			source.Cancel();

			var result = await task.ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(CanceledResult));
		}

		[TestMethod]
		public async Task ProcessModifyVolume_Test()
		{
			using var temp = new TempDirectory();

			var (anime, song) = GenerateAnimeAndSong(temp.Dir);
			song.VolumeModifier = VolumeModifer.FromDecibels(-5);
			var job = new Mp3SongJob(anime, song);
			var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);

			var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
			Assert.IsTrue(ValidVideoVolume.NSamples / DIV >= newVolumeInfo.Info.NSamples);
			Assert.IsTrue(ValidVideoVolume.MaxVolume > newVolumeInfo.Info.MaxVolume);
			Assert.IsTrue(ValidVideoVolume.MeanVolume > newVolumeInfo.Info.MeanVolume);
		}

		[TestMethod]
		public async Task ProcessWithClean_Test()
		{
		}

		private static async Task<string> GetSingleFileProducedAsync(string directory, ISongJob job)
		{
			var result = await job.ProcessAsync().ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			var files = Directory.GetFiles(directory);
			Assert.AreEqual(1, files.Length);
			return files[0];
		}

		private Anime GenerateAnime(string directory)
		{
			return new Anime(Path.Combine(directory, "info.amq"), new AnimeBase
			{
				Id = 73,
				Name = "Extremely Long Light Novel Title",
				Songs = new(),
				Source = ValidVideoPath,
				Year = 2500
			}, new(ValidVideoPath, ValidVideoInfo with
			{
				DAR = new(16, 9),
				SAR = AspectRatio.Square,
			}));
		}

		private (Anime, Song) GenerateAnimeAndSong(string directory)
		{
			var anime = GenerateAnime(directory);
			var song = new Song()
			{
				Start = TimeSpan.FromSeconds(0),
				End = TimeSpan.FromSeconds(anime.VideoInfo!.Value.Info.Duration!.Value / DIV),
				Name = Guid.NewGuid().ToString(),
			};
			return (anime, song);
		}
	}
}