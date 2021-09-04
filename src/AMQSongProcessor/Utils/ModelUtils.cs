
using AMQSongProcessor.Models;

namespace AMQSongProcessor.Utils
{
	public static class ModelUtils
	{
		public static string GetAbsoluteSourcePath(this IAnime anime)
			=> FileUtils.EnsureAbsolutePath(anime.GetDirectory(), anime.Source)!;

		public static string? GetCleanSongPath(this ISong song, string directory)
			=> FileUtils.EnsureAbsolutePath(directory, song.CleanPath);

		public static string GetDirectory(this IAnime anime)
			=> Path.GetDirectoryName(anime.AbsoluteInfoPath)!;

		public static string GetFullName(this ISong song)
			=> $"{song.Name} ({song.Artist})";

		public static TimeSpan GetLength(this ISong song)
			=> song.End - song.Start;

		public static string GetMp3Path(this ISong song, string directory, int animeId)
			=> FileUtils.EnsureAbsolutePath(directory, $"[{animeId}] {song.Name}.mp3")!;

		public static string? GetRelativeOrAbsoluteSourcePath(this IAnime anime)
			=> FileUtils.GetRelativeOrAbsolute(anime.GetDirectory(), anime.VideoInfo?.Path);

		public static string GetVideoPath(this ISong song, string directory, int animeId, int resolution)
			=> FileUtils.EnsureAbsolutePath(directory, $"[{animeId}] {song.Name} [{resolution}p].webm")!;

		public static bool HasTimeStamp(this ISong song)
			=> song.Start > TimeSpan.FromSeconds(0);

		public static bool IsMissing(this ISong song, Status status)
			=> (song.Status & status) == 0;

		public static bool IsUnsubmitted(this ISong song)
			=> song.Status == Status.NotSubmitted;
	}
}