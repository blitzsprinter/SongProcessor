using AMQSongProcessor.Models;

namespace AMQSongProcessor.Warnings
{
	public sealed class SourceIsNull : IWarning
	{
		public IAnime Anime { get; }

		public SourceIsNull(IAnime anime)
		{
			Anime = anime;
		}

		public override string ToString()
			=> $"Source is null: {Anime.Name}";
	}
}