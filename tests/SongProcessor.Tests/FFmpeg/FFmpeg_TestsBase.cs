using SongProcessor.FFmpeg;
using SongProcessor.Models;
using SongProcessor.Tests.Properties;

namespace SongProcessor.Tests.FFmpeg;

public abstract class FFmpeg_TestsBase
{
	public const string FFMPEG_CATEGORY = "FFmpeg";
	public const string FFPROBE_CATEGORY = "FFprobe";
	protected static string VideoPath { get; } = Path.Combine(
		Directory.GetCurrentDirectory(),
		nameof(Resources),
		$"{nameof(Resources.ValidVideo)}.mp4"
	);
	protected AudioInfo AudioInfo { get; set; } = new(VideoPath);
	protected SourceInfoGatherer Gatherer { get; set; } = new();
	protected VideoInfo VideoInfo { get; set; } = new(
		File: VideoPath,
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
		Tags: new()
		{
			["creation_time"] = "2020-07-23T18:42:17.000000Z",
			["language"] = "und",
			["handler_name"] = "Vireo Eyes v2.5.3",
			["vendor_id"] = "[0][0][0][0]",
			["encoder"] = "AVC Coding",
		},
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
	protected VolumeInfo VolumeInfo { get; set; } = new(
		File: VideoPath,
		Histograms: new() { [0] = 25099 },
		MaxVolume: 0,
		MeanVolume: -6.1,
		NSamples: 250880
	);

	protected virtual Anime CreateAnime(string directory)
	{
		return new Anime(Path.Combine(directory, "info.amq"), new AnimeBase
		{
			Id = 73,
			Name = "Extremely Long Light Novel Title",
			Songs = new(),
			Source = VideoInfo.File,
			Year = 2500
		}, VideoInfo);
	}
}