
using Newtonsoft.Json;

namespace AMQSongProcessor.UI
{
	public sealed class NewtonsoftJsonSkipThis : JsonConverter
	{
		public override bool CanConvert(Type objectType)
			=> true;

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
			=> throw new NotSupportedException();

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
		}
	}
}