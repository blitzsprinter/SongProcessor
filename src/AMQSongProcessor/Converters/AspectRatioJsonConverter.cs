using System.Text.Json;
using System.Text.Json.Serialization;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Converters
{
	internal sealed class AspectRatioJsonConverter : JsonConverter<AspectRatio>
	{
		private const char SEPARATOR = ':';

		public override AspectRatio Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.GetString() is not string s)
			{
				return default;
			}

			var values = s.Split(SEPARATOR);
			var width = int.Parse(values[0]);
			var height = int.Parse(values[1]);
			return new AspectRatio(width, height);
		}

		public override void Write(Utf8JsonWriter writer, AspectRatio value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString(SEPARATOR));
	}
}