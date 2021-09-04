namespace AMQSongProcessor
{
	[Flags]
	public enum IgnoreExceptions : uint
	{
		None = 0,
		Json = (1U << 0),
		Video = (1U << 1),
		All = Json | Video,
	}
}