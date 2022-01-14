using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg;

namespace SongProcessor.Tests.FFmpeg;

[TestClass]
public sealed class SourceInfoGatherer_Tests : FFmpeg_TestsBase
{
	[TestMethod]
	[TestCategory(FFPROBE_CATEGORY)]
	public async Task GetAudioInfo_Test()
	{
		var actual = await Gatherer.GetAudioInfoAsync(ValidVideoFile).ConfigureAwait(false);
		actual.File.Should().Be(ValidVideoFile);
		actual.Info.Should().Be(new AudioInfo());
		// uncomment below if volumeinfo implemented
		//actual.Info.Should().BeEquivalentTo(new AudioInfo());
	}

	[TestMethod]
	public async Task GetAudioInfoNonExistentFile_Test()
	{
		Func<Task> getInfo = () => Gatherer.GetAudioInfoAsync(FakeFileName);
		await getInfo.Should().ThrowAsync<SourceInfoGatheringException>().ConfigureAwait(false);
	}

	[TestMethod]
	[TestCategory(FFPROBE_CATEGORY)]
	public async Task GetVideoInfo_Test()
	{
		var actual = await Gatherer.GetVideoInfoAsync(ValidVideoFile).ConfigureAwait(false);
		actual.File.Should().Be(ValidVideoFile);
		actual.Info.Should().BeEquivalentTo(ValidVideoInfo);
	}

	[TestMethod]
	public async Task GetVideoInfoNonExistentFile_Test()
	{
		Func<Task> getInfo = () => Gatherer.GetVideoInfoAsync(FakeFileName);
		await getInfo.Should().ThrowAsync<SourceInfoGatheringException>().ConfigureAwait(false);
	}

	[TestMethod]
	[TestCategory(FFMPEG_CATEGORY)]
	public async Task GetVolumeInfo_Test()
	{
		var actual = await Gatherer.GetVolumeInfoAsync(ValidVideoFile).ConfigureAwait(false);
		actual.File.Should().Be(ValidVideoFile);
		actual.Info.Should().BeEquivalentTo(ValidVideoVolume);
	}

	[TestMethod]
	public async Task GetVolumeInfoNonExistentFile_Test()
	{
		Func<Task> getInfo = () => Gatherer.GetVolumeInfoAsync(FakeFileName);
		await getInfo.Should().ThrowAsync<SourceInfoGatheringException>().ConfigureAwait(false);
	}
}