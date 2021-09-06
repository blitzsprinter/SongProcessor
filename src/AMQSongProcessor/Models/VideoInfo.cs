using System.Text.Json.Serialization;

namespace AMQSongProcessor.Models
{
	public sealed record VideoInfo(
		[property: JsonPropertyName("avg_frame_rate")]
		string AverageFrameRate,
		[property: JsonPropertyName("chroma_location")]
		string ChromaLocation,
		[property: JsonPropertyName("codec_long_name")]
		string CodecLongName,
		[property: JsonPropertyName("codec_name")]
		string CodecName,
		[property: JsonPropertyName("codec_tag")]
		string CodecTag,
		[property: JsonPropertyName("codec_tag_string")]
		string CodecTagString,
		[property: JsonPropertyName("codec_time_base")]
		string CodecTimeBase,
		[property: JsonPropertyName("codec_type")]
		string CodecType,
		[property: JsonPropertyName("coded_height")]
		int CodedHeight,
		[property: JsonPropertyName("coded_width")]
		int CodedWidth,
		[property: JsonPropertyName("color_range")]
		string ColorRange,
		[property: JsonPropertyName("display_aspect_ratio")]
		AspectRatio DAR,
		[property: JsonPropertyName("has_b_frames")]
		int HasBFrames,
		[property: JsonPropertyName("height")]
		int Height,
		[property: JsonPropertyName("index")]
		int Index,
		[property: JsonPropertyName("level")]
		int Level,
		[property: JsonPropertyName("pix_fmt")]
		string PixelFormat,
		[property: JsonPropertyName("profile")]
		string Profile,
		[property: JsonPropertyName("refs")]
		int Ref,
		[property: JsonPropertyName("r_frame_rate")]
		string RFrameRate,
		[property: JsonPropertyName("sample_aspect_ratio")]
		AspectRatio SAR,
		[property: JsonPropertyName("start_pts")]
		int StartPoints,
		[property: JsonPropertyName("start_time")]
		string StartTime,
		[property: JsonPropertyName("time_base")]
		string TimeBase,
		[property: JsonPropertyName("width")]
		int Width
	);
}