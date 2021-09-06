namespace AMQSongProcessor.Models
{
	internal static class ModelUtils
	{
		internal static FormatException InvalidFormat<T>(string? s)
			=> new($"Invalid {typeof(T).Name} format: {s}.");
	}
}