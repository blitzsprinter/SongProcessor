using System.Text.Json;
using System.Text.Json.Serialization;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Converters
{
	internal sealed class SongTypeAndPositionJsonConverter : JsonConverter<SongTypeAndPosition>
	{
		public override SongTypeAndPosition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> reader.GetString() is string s ? SongTypeAndPosition.Parse(s) : default;

		public override void Write(Utf8JsonWriter writer, SongTypeAndPosition value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString(shortType: true));
	}
}