using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Converters
{
	public sealed class SongTypeAndPositionJsonConverter : JsonConverter<SongTypeAndPosition>
	{
		public override SongTypeAndPosition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var value = reader.GetString();
			var pos = GetFirstDigitPosition(value);

			var type = Enum.Parse<SongType>(value[..pos]);
			var position = pos == value.Length ? default(int?) : int.Parse(value[pos..]);
			return new SongTypeAndPosition(type, position);
		}

		public override void Write(Utf8JsonWriter writer, SongTypeAndPosition value, JsonSerializerOptions options)
		{
			var s = value.ShortType;
			if (value.Position != null)
			{
				s += value.Position.ToString();
			}

			writer.WriteStringValue(s);
		}

		private static int GetFirstDigitPosition(string value)
		{
			for (var i = 0; i < value.Length; ++i)
			{
				if (char.IsDigit(value[i]))
				{
					return i;
				}
			}
			return value.Length;
		}
	}
}