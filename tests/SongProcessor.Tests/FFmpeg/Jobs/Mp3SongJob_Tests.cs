using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg;
using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Results;

namespace SongProcessor.Tests.FFmpeg.Jobs;

[TestClass]
public sealed class Mp3SongJob_Tests : SongJob_TestsBase
{
	[TestMethod]
	public async Task ProcessMp3_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			return new Mp3SongJob(anime, song);
		});

		var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVolumeInfo.Info);

		job.AlreadyExists.Should().BeTrue();
		var result = await job.ProcessAsync().ConfigureAwait(false);
		result.IsSuccess.Should().BeFalse();
		result.Should().BeOfType<FileAlreadyExists>();
	}

	[TestMethod]
	public async Task ProcessMp3Canceled_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			return new Mp3SongJob(anime, song);
		});
		var cts = new CancellationTokenSource();

		var task = job.ProcessAsync(cts.Token);
		cts.Cancel();

		var result = await task.ConfigureAwait(false);
		result.IsSuccess.Should().BeNull();
		result.Should().BeOfType<Canceled>();
	}

	[TestMethod]
	public async Task ProcessMp3ModifyVolume_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			song.VolumeModifier = VolumeModifer.FromDecibels(-5);
			return new Mp3SongJob(anime, song);
		});

		var file = await GetSingleFileProducedAsync(temp.Dir, job).ConfigureAwait(false);
		var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVolumeInfo.Info);
		newVolumeInfo.Info.MaxVolume.Should().BeLessThan(ValidVideoVolume.MaxVolume);
		newVolumeInfo.Info.MeanVolume.Should().BeLessThan(ValidVideoVolume.MeanVolume);
	}

	[TestMethod]
	public async Task ProcessMp3WithClean_Test()
	{
		using var temp = new TempDirectory();
		var job = GenerateJob(temp.Dir, (anime, song) =>
		{
			// Generate a clean version from the entire video
			song.End = TimeSpan.FromSeconds(anime.VideoInfo!.Value.Info.Duration!.Value);
			return new Mp3SongJob(anime, song);
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

		job = GenerateJob(temp.Dir, (anime, song) =>
		{
			// Use the newly created clean path
			song.CleanPath = cleanPath;
			return new Mp3SongJob(anime, song);
		});

		var result = await job.ProcessAsync().ConfigureAwait(false);
		result.IsSuccess.Should().BeTrue();
		// Delete the clean version so we can get the output easier
		File.Delete(cleanPath);

		var file = GetSingleFile(temp.Dir);
		var newVolumeInfo = await Gatherer.GetVolumeInfoAsync(file).ConfigureAwait(false);
		AssertValidLength(job, newVolumeInfo.Info);
	}

	private void AssertValidLength(SongJob job, VolumeInfo info)
	{
		var duration = (double)info.NSamples;
		var divisor = ValidVideoInfo.Duration / job.Song.GetLength().TotalSeconds;
		var expected = (ValidVideoVolume.NSamples / divisor)!.Value;
		AssertValidLength(duration, expected);
	}
}