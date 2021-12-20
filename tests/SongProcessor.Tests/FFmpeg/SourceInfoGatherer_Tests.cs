using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg;

namespace SongProcessor.Tests.FFmpeg;

[TestClass]
[TestCategory(FFMPEG_CATEGORY)]
public sealed class SourceInfoGatherer_Tests : FFmpeg_TestsBase
{
	[TestMethod]
	public async Task GetAudioInfo_Test()
	{
		var actual = await Gatherer.GetAudioInfoAsync(ValidVideoPath).ConfigureAwait(false);
		Assert.IsNotNull(actual.Info);
		Assert.AreEqual(ValidVideoPath, actual.Path);
	}

	[TestMethod]
	public async Task GetAudioInfoNonExistentFile_Test()
	{
		await Assert.ThrowsExceptionAsync<SourceInfoGatheringException>(async () =>
		{
			_ = await Gatherer.GetAudioInfoAsync(NonExistentFileName).ConfigureAwait(false);
		}).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task GetVideoInfo_Test()
	{
		var actual = await Gatherer.GetVideoInfoAsync(ValidVideoPath).ConfigureAwait(false);
		Assert.IsNotNull(actual.Info);
		Assert.AreEqual(ValidVideoPath, actual.Path);

		foreach (var property in typeof(VideoInfo).GetProperties())
		{
			if (property.Name.Equals(nameof(VideoInfo.Tags)))
			{
				var expectedValue = ValidVideoInfo.Tags;
				var actualValue = actual.Info.Tags;
				CollectionAssert.AreEqual(expectedValue, actualValue, property.Name);
			}
			else
			{
				var expectedValue = property.GetValue(ValidVideoInfo);
				var actualValue = property.GetValue(actual.Info);
				Assert.AreEqual(expectedValue, actualValue, property.Name);
			}
		}
	}

	[TestMethod]
	public async Task GetVideoInfoNonExistentFile_Test()
	{
		await Assert.ThrowsExceptionAsync<SourceInfoGatheringException>(async () =>
		{
			_ = await Gatherer.GetVideoInfoAsync(NonExistentFileName).ConfigureAwait(false);
		}).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task GetVolumeInfo_Test()
	{
		var actual = await Gatherer.GetVolumeInfoAsync(ValidVideoPath).ConfigureAwait(false);
		Assert.IsNotNull(actual.Info);
		Assert.AreEqual(ValidVideoPath, actual.Path);

		var info = actual.Info;
		Assert.AreEqual(0, info.Histograms.Keys.Single());
		Assert.AreEqual(25099, info.Histograms.Values.Single());
		Assert.AreEqual(0, info.MaxVolume);
		Assert.AreEqual(-6.1, info.MeanVolume);
		Assert.AreEqual(250880, info.NSamples);
	}

	[TestMethod]
	public async Task GetVolumeInfoNonExistentFile_Test()
	{
		await Assert.ThrowsExceptionAsync<SourceInfoGatheringException>(async () =>
		{
			_ = await Gatherer.GetVolumeInfoAsync(NonExistentFileName).ConfigureAwait(false);
		}).ConfigureAwait(false);
	}
}