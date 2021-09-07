using System.Text.Json;
using System.Text.Json.Serialization;

namespace AMQSongProcessor.Converters
{
	public sealed class ParseJsonConverter<T> : JsonConverter<T>
	{
		private readonly Func<string, T> _Parse;

		public ParseJsonConverter(Func<string, T> parse)
		{
			_Parse = parse;
		}

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> _Parse(reader.GetString()!);

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value?.ToString());
	}
}