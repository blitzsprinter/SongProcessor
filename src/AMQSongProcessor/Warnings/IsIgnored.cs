using AMQSongProcessor.Models;

namespace AMQSongProcessor.Warnings
{
	public sealed class IsIgnored : IWarning
	{
		public Song Song { get; }

		public IsIgnored(Song song)
		{
			Song = song;
		}

		public override string ToString()
			=> $"Is ignored: {Song.Name}";
	}
}