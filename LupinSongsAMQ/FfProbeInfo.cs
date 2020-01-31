using System.Text.Json.Serialization;

namespace LupinSongsAMQ
{
	public sealed class FfProbeInfo
	{
		[JsonPropertyName("avg_frame_rate")]
		public string AverageFrameRate { get; set; }

		[JsonPropertyName("chroma_location")]
		public string ChromaLocation { get; set; }

		[JsonPropertyName("codec_long_name")]
		public string CodecLongName { get; set; }

		[JsonPropertyName("codec_name")]
		public string CodecName { get; set; }

		[JsonPropertyName("codec_tag")]
		public string CodecTag { get; set; }

		[JsonPropertyName("codec_tag_string")]
		public string CodecTagString { get; set; }

		[JsonPropertyName("codec_time_base")]
		public string CodecTimeBase { get; set; }

		[JsonPropertyName("codec_type")]
		public string CodecType { get; set; }

		[JsonPropertyName("coded_height")]
		public int CodedHeight { get; set; }

		[JsonPropertyName("coded_width")]
		public int CodedWidth { get; set; }

		[JsonPropertyName("color_range")]
		public string ColorRange { get; set; }

		[JsonPropertyName("display_aspect_ratio")]
		[JsonConverter(typeof(AspectRatioJsonConverter))]
		public AspectRatio DAR { get; set; }

		[JsonPropertyName("has_b_frames")]
		public int HasBFrames { get; set; }

		[JsonPropertyName("height")]
		public int Height { get; set; }

		[JsonPropertyName("index")]
		public int Index { get; set; }

		[JsonPropertyName("level")]
		public int Level { get; set; }

		[JsonPropertyName("pix_fmt")]
		public string PixelFormat { get; set; }

		[JsonPropertyName("profile")]
		public string Profile { get; set; }

		[JsonPropertyName("refs")]
		public int Ref { get; set; }

		[JsonPropertyName("r_frame_rate")]
		public string RFrameRate { get; set; }

		[JsonPropertyName("sample_aspect_ratio")]
		[JsonConverter(typeof(AspectRatioJsonConverter))]
		public AspectRatio SAR { get; set; }

		[JsonPropertyName("start_pts")]
		public int StartPoints { get; set; }

		[JsonPropertyName("start_time")]
		public string StartTime { get; set; }

		[JsonPropertyName("time_base")]
		public string TimeBase { get; set; }

		[JsonPropertyName("width")]
		public int Width { get; set; }
	}
}