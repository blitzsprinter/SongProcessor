using FluentAssertions;

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
		actual.Path.Should().Be(ValidVideoPath);
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
	public async Task GetVideoInfo_Test()
	{
		var actual = await Gatherer.GetVideoInfoAsync(ValidVideoPath).ConfigureAwait(false);
		actual.Path.Should().Be(ValidVideoPath);
		actual.Info.Should().BeEquivalentTo(ValidVideoInfo);
	}

	[TestMethod]
	public async Task GetVideoInfoNonExistentFile_Test()
	{
		Func<Task> getInfo = () => Gatherer.GetVideoInfoAsync(FakeFileName);
		await getInfo.Should().ThrowAsync<SourceInfoGatheringException>().ConfigureAwait(false);
	}

	[TestMethod]
	public async Task GetVolumeInfo_Test()
	{
		var actual = await Gatherer.GetVolumeInfoAsync(ValidVideoPath).ConfigureAwait(false);
		actual.Path.Should().Be(ValidVideoPath);
		actual.Info.Should().BeEquivalentTo(ValidVideoVolume);
	}

	[TestMethod]
	public async Task GetVolumeInfoNonExistentFile_Test()
	{
		Func<Task> getInfo = () => Gatherer.GetVolumeInfoAsync(FakeFileName);
		await getInfo.Should().ThrowAsync<SourceInfoGatheringException>().ConfigureAwait(false);
	}
}