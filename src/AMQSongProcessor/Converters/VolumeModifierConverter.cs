using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Converters
{
	public sealed class VolumeModifierConverter : JsonConverter<VolumeModifer?>
	{
		public override VolumeModifer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var s = reader.GetString();
			if (s == null)
			{
				return null;
			}
			return VolumeModifer.Parse(s);
		}

		public override void Write(Utf8JsonWriter writer, VolumeModifer? value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value?.ToString());
	}
}