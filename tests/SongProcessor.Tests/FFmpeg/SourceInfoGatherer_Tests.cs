using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg;

namespace SongProcessor.Tests.FFmpeg;

[TestClass]
public sealed class SourceInfoGatherer_Tests : FFmpeg_TestsBase
{
	private const string FAKE_FILE = "DoesNotExist.txt";

	[TestMethod]
	[TestCategory(FFPROBE_CATEGORY)]
	public async Task GetAudioInfo_Test()
	{
		var actual = await Gatherer.GetAudioInfoAsync(VideoInfo.File).ConfigureAwait(false);
		actual.Should().BeEquivalentTo(AudioInfo,
			x => x.ComparingByMembers<AudioInfo>());
	}

	[TestMethod]
	public async Task GetAudioInfoNonExistentFile_Test()
	{
		Func<Task> getInfo = () => Gatherer.GetAudioInfoAsync(FAKE_FILE);
		await getInfo.Should().ThrowAsync<SourceInfoGatheringException>().ConfigureAwait(false);
	}

	[TestMethod]
	[TestCategory(FFPROBE_CATEGORY)]
	public async Task GetVideoInfo_Test()
	{
		var actual = await Gatherer.GetVideoInfoAsync(VideoInfo.File).ConfigureAwait(false);
		actual.Should().BeEquivalentTo(VideoInfo,
			x => x.ComparingByMembers<VideoInfo>());
	}

	[TestMethod]
	public async Task GetVideoInfoNonExistentFile_Test()
	{
		Func<Task> getInfo = () => Gatherer.GetVideoInfoAsync(FAKE_FILE);
		await getInfo.Should().ThrowAsync<SourceInfoGatheringException>().ConfigureAwait(false);
	}

	[TestMethod]
	[TestCategory(FFMPEG_CATEGORY)]
	public async Task GetVolumeInfo_Test()
	{
		var actual = await Gatherer.GetVolumeInfoAsync(VideoInfo.File).ConfigureAwait(false);
		actual.Should().BeEquivalentTo(VolumeInfo,
			x => x.ComparingByMembers<VolumeInfo>());
	}

	[TestMethod]
	public async Task GetVolumeInfoNonExistentFile_Test()
	{
		Func<Task> getInfo = () => Gatherer.GetVolumeInfoAsync(FAKE_FILE);
		await getInfo.Should().ThrowAsync<SourceInfoGatheringException>().ConfigureAwait(false);
	}
}