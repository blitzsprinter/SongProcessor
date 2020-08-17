using System.IO;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Utils
{
	public static class AnimeUtils
	{
		public static string GetAbsoluteSourcePath(this IAnime anime)
			=> FileUtils.EnsureAbsolutePath(anime.GetDirectory(), anime.Source)!;

		public static string GetDirectory(this IAnime anime)
			=> Path.GetDirectoryName(anime.AbsoluteInfoPath)!;
	}
}