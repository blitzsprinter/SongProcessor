using SongProcessor.Models;

namespace SongProcessor.Warnings
{
	public sealed class VideoTooSmall : IWarning
	{
		public IAnime Anime { get; }
		public int Resolution { get; }

		public VideoTooSmall(IAnime anime, int resolution)
		{
			Anime = anime;
			Resolution = resolution;
		}

		public override string ToString()
			=> $"Source is smaller than {Resolution}p: {Anime.Name}";
	}
}