using System.Text.Json.Serialization;

using SongProcessor.Models;

namespace SongProcessor.FFmpeg;

public sealed record VideoInfo(
	[property: JsonPropertyName("avg_frame_rate")]
		string AverageFrameRate,
	[property: JsonPropertyName("closed_captions")]
		int ClosedCaptions,
	[property: JsonPropertyName("codec_long_name")]
		string CodecLongName,
	[property: JsonPropertyName("codec_name")]
		string CodecName,
	[property: JsonPropertyName("codec_tag")]
		string CodecTag,
	[property: JsonPropertyName("codec_tag_string")]
		string CodecTagString,
	[property: JsonPropertyName("codec_type")]
		string CodecType,
	[property: JsonPropertyName("coded_height")]
		int CodedHeight,
	[property: JsonPropertyName("coded_width")]
		int CodedWidth,
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
	[property: JsonPropertyName("refs")]
		int Refs,
	[property: JsonPropertyName("r_frame_rate")]
		string RFrameRate,
	[property: JsonPropertyName("start_pts")]
		int StartPoints,
	[property: JsonPropertyName("start_time")]
		string StartTime,
	[property: JsonPropertyName("time_base")]
		string TimeBase,
	[property: JsonPropertyName("width")]
		int Width,
	[property: JsonPropertyName("tags")]
		Dictionary<string, string> Tags,
	[property: JsonPropertyName("bit_rate")]
		int? Bitrate = null,
	[property: JsonPropertyName("bits_per_raw_sample")]
		int? BitsPerRawSample = null,
	[property: JsonPropertyName("chroma_location")]
		string? ChromaLocation = null,
	[property: JsonPropertyName("codec_time_base")]
		string? CodecTimeBase = null,
	[property: JsonPropertyName("color_primaries")]
		string? ColorPrimaries = null,
	[property: JsonPropertyName("color_range")]
		string? ColorRange = null,
	[property: JsonPropertyName("color_space")]
		string? ColorSpace = null,
	[property: JsonPropertyName("color_transfer")]
		string? ColorTransfer = null,
	[property: JsonPropertyName("display_aspect_ratio")]
		AspectRatio? DAR = null,
	[property: JsonPropertyName("divx_packed")]
		bool? DivxPacked = null,
	[property: JsonPropertyName("duration")]
		double? Duration = null,
	[property: JsonPropertyName("duration_ts")]
		long? DurationTicks = null,
	[property: JsonPropertyName("field_order")]
		string? FieldOrder = null,
	[property: JsonPropertyName("is_avc")]
		bool? IsAvc = null,
	[property: JsonPropertyName("nal_length_size")]
		int? NalLengthSize = null,
	[property: JsonPropertyName("nb_frames")]
		int? NbFrames = null,
	[property: JsonPropertyName("profile")]
		string? Profile = null,
	[property: JsonPropertyName("quarter_sample")]
		bool? QuarterSample = null,
	[property: JsonPropertyName("sample_aspect_ratio")]
		AspectRatio? SAR = null
)
{
}
