using AMQSongProcessor.Models;

namespace AMQSongProcessor.Warnings
{
	public sealed class TimestampIsNull : IWarning
	{
		public ISong Song { get; }

		public TimestampIsNull(ISong song)
		{
			Song = song;
		}

		public override string ToString()
			=> $"Timestamp is null: {Song.Name}";
	}
}