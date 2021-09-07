using AMQSongProcessor.FFmpeg;
using AMQSongProcessor.Tests.Properties;

namespace AMQSongProcessor.Tests.FFmpeg
{
	public abstract class FFmpeg_TestsBase
	{
		public const string FFMPEG_CATEGORY = "FFmpeg";
		public virtual SourceInfoGatherer Gatherer { get; } = new();
		public virtual string NonExistentFileName { get; } = "DoesNotExist.txt";
		public virtual VideoInfo ValidVideoInfo { get; } = new VideoInfo(
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
		public virtual string ValidVideoPath { get; } = Path.Combine(
			Directory.GetCurrentDirectory(),
			nameof(Resources),
			$"{nameof(Resources.ValidVideo)}.mp4"
		);
		public virtual VolumeInfo ValidVideoVolume { get; } = new(
			Histograms: new() { [0] = 25099 },
			MaxVolume: 0,
			MeanVolume: -6.1,
			NSamples: 250880
		);
	}
}