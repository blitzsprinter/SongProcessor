using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Results;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SongProcessor.Tests.FFmpeg.Jobs
{
	[TestClass]
	[TestCategory(FFMPEG_CATEGORY)]
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
			Assert.IsInstanceOfType(result, typeof(FileAlreadyExists));
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
			Assert.IsNull(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(Canceled));
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
			using var temp = new TempDirectory();

			var (anime, song) = GenerateAnimeAndSong(temp.Dir);
			song.End = TimeSpan.FromSeconds(anime.VideoInfo!.Value.Info.Duration!.Value);
			var job = new Mp3SongJob(anime, song);
			// Create a duplicate version to treat as a clean version
			var cleanPath = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
			var cleanVolumeInfo = await Gatherer.GetVolumeInfoAsync(cleanPath).ConfigureAwait(false);
			Assert.AreEqual(ValidVideoVolume.NSamples, cleanVolumeInfo.Info.NSamples, 2000);
			{
				var movedPath = Path.Combine(
					Path.GetDirectoryName(cleanPath)!,
					$"clean{Path.GetExtension(cleanPath)}");
				File.Move(cleanPath, movedPath);
				cleanPath = movedPath;
			}

			// Set the length back to being shorter than the produced clean version
			song.End = TimeSpan.FromSeconds(anime.VideoInfo!.Value.Info.Duration!.Value / DIV);
			song.CleanPath = cleanPath;
			var result = await job.ProcessAsync().ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);

			// Delete the clean version so we can get the output easier
			File.Delete(cleanPath);
			var file = GetSingleFile(temp.Dir);

			var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
			Assert.IsTrue(ValidVideoVolume.NSamples / DIV >= newVolumeInfo.Info.NSamples);
		}

		private static string GetSingleFile(string directory)
		{
			var files = Directory.GetFiles(directory);
			Assert.AreEqual(1, files.Length);
			return files.Single();
		}

		private static async Task<string> GetSingleFileProducedAsync(string directory, ISongJob job)
		{
			var result = await job.ProcessAsync().ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			return GetSingleFile(directory);
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