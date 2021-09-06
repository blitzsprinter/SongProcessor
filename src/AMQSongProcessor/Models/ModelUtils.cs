namespace AMQSongProcessor.Models
{
	public static class ModelUtils
	{
		public static SongTypeAndPosition Create(this SongType type, int? position)
			=> new(type, position);

		internal static FormatException InvalidFormat<T>(string? s)
			=> new($"Invalid {typeof(T).Name} format: {s}.");
	}
}