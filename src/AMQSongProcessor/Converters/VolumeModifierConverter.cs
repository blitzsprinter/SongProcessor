using System.Text.Json;
using System.Text.Json.Serialization;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Converters
{
	internal sealed class VolumeModifierConverter : JsonConverter<VolumeModifer?>
	{
		public override VolumeModifer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> reader.GetString() is string s ? VolumeModifer.Parse(s) : default;

		public override void Write(Utf8JsonWriter writer, VolumeModifer? value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value?.ToString());
	}
}