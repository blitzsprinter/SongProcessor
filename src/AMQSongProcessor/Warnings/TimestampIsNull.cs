using AMQSongProcessor.Models;

namespace AMQSongProcessor.Warnings
{
	public sealed class TimestampIsNull : IWarning
	{
		public Song Song { get; }

		public TimestampIsNull(Song song)
		{
			Song = song;
		}

		public override string ToString()
			=> $"Timestamp is null: {Song.Name}";
	}
}