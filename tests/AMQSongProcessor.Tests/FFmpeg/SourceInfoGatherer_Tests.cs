using AMQSongProcessor.FFmpeg;
using AMQSongProcessor.Tests.Properties;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.FFmpeg
{
	[TestClass]
	[TestCategory("FFmpeg")]
	public sealed class SourceInfoGatherer_Tests
	{
		public const string NonExistentPath = "DoesNotExist.txt";
		public SourceInfoGatherer Gatherer { get; } = new();
		public string ValidVideoPath { get; } = Path.Combine(
			Directory.GetCurrentDirectory(),
			nameof(Resources),
			$"{nameof(Resources.ValidVideo)}.mp4"
		);

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
				_ = await Gatherer.GetAudioInfoAsync(NonExistentPath).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		[TestMethod]
		public async Task GetVideoInfo_Test()
		{
			var actual = await Gatherer.GetVideoInfoAsync(ValidVideoPath).ConfigureAwait(false);
			Assert.IsNotNull(actual.Info);
			Assert.AreEqual(ValidVideoPath, actual.Path);

			var expected = new VideoInfo(
				AverageFrameRate: "24000/1001",
				ClosedCaptions: 0,
				CodecLongName: "H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10",
				CodecName: "h264",
				CodecTag: "0x31637661",
				CodecTagString: "avc1",
				CodecType: "video",
				CodedHeight: 270,
				CodedWidth: 360,
				HasBFrames: 2,
				Height: 270,
				Index: 0,
				Level: 13,
				PixelFormat: "yuv420p",
				Refs: 1,
				RFrameRate: "24000/1001",
				StartPoints: 0,
				StartTime: "0.000000",
				TimeBase: "1/24000",
				Width: 360,
				// Optional
				Bitrate: 11338,
				BitsPerRawSample: 8,
				ChromaLocation: "left",
				Duration: 5.213542,
				DurationTicks: 125125,
				IsAvc: true,
				NalLengthSize: 4,
				NbFrames: 125,
				Profile: "Main"
			);

			foreach (var property in typeof(VideoInfo).GetProperties())
			{
				var expectedValue = property.GetValue(expected);
				var actualValue = property.GetValue(actual.Info);
				Assert.AreEqual(expectedValue, actualValue, property.Name);
			}
		}

		[TestMethod]
		public async Task GetVideoInfoNonExistentFile_Test()
		{
			await Assert.ThrowsExceptionAsync<SourceInfoGatheringException>(async () =>
			{
				_ = await Gatherer.GetVideoInfoAsync(NonExistentPath).ConfigureAwait(false);
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
				_ = await Gatherer.GetVolumeInfoAsync(NonExistentPath).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}
	}
}