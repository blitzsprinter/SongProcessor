using System.Text.Json;
using System.Text.Json.Serialization;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Converters
{
	internal sealed class AspectRatioJsonConverter : JsonConverter<AspectRatio>
	{
		private const char SEPARATOR = ':';

		public override AspectRatio Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> reader.GetString() is string s ? AspectRatio.Parse(s, SEPARATOR) : default;

		public override void Write(Utf8JsonWriter writer, AspectRatio value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString(SEPARATOR));
	}
}