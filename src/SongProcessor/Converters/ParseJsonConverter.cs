using System.Text.Json;
using System.Text.Json.Serialization;

namespace SongProcessor.Converters;

public sealed class ParseJsonConverter<T>(Func<string, T> Parse) : JsonConverter<T>
{
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> Parse(reader.GetString()!);

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		=> writer.WriteStringValue(value?.ToString());
}