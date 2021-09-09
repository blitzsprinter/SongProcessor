using SongProcessor.Models;

namespace SongProcessor.Warnings
{
	public sealed class IsIgnored : IWarning
	{
		public ISong Song { get; }

		public IsIgnored(ISong song)
		{
			Song = song;
		}

		public override string ToString()
			=> $"Is ignored: {Song.Name}";
	}
}