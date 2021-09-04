using System.Text.Json;
using System.Text.Json.Serialization;

namespace AMQSongProcessor.Converters
{
	internal sealed class TimeSpanJsonConverter : JsonConverter<TimeSpan>
	{
		public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> reader.GetString() is string s ? TimeSpan.Parse(s) : default;

		public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString());
	}
}