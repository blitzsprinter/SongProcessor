using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Results;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SongProcessor.Tests.FFmpeg.Jobs
{
	[TestClass]
	public sealed class Mp3SongJob_Tests : SongJob_TestsBase
	{
		[TestMethod]
		public async Task Process_Test()
		{
			using var temp = new TempDirectory();

			var (anime, song) = GenerateAnimeAndSong(temp.Dir);
			var job = new Mp3SongJob(anime, song);
			var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);

			var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
			var expected = ValidVideoVolume.NSamples / DIV;
			Assert.AreEqual(expected, newVolumeInfo.Info.NSamples, expected * 0.05);

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
			var expected = ValidVideoVolume.NSamples / DIV;
			Assert.AreEqual(expected, newVolumeInfo.Info.NSamples, expected * 0.05);
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
			var expected = ValidVideoVolume.NSamples / DIV;
			Assert.AreEqual(expected, newVolumeInfo.Info.NSamples, expected * 0.05);
		}
	}
}