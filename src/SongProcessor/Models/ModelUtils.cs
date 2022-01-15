using SongProcessor.Utils;

namespace SongProcessor.Models;

public static class ModelUtils
{
	public const string DEBUGGER_DISPLAY = "{DebuggerDisplay,nq}";

	public static SongTypeAndPosition Create(this SongType type, int? position)
		=> new(type, position);

	public static string? GetCleanFile(this ISong song, IAnime anime)
		=> FileUtils.EnsureAbsoluteFile(anime.GetDirectory(), song.CleanPath);

	public static string GetDirectory(this IAnime anime)
		=> Path.GetDirectoryName(anime.AbsoluteInfoPath)!;

	public static string GetFullName(this ISong song)
		=> $"{song.Name} ({song.Artist})";

	public static TimeSpan GetLength(this ISong song)
		=> song.End - song.Start;

	public static string GetMp3File(this ISong song, IAnime anime)
		=> FileUtils.EnsureAbsoluteFile(anime.GetDirectory(), $"[{anime.Id}] {song.Name}.mp3")!;

	public static string? GetRelativeOrAbsoluteSourceFile(this IAnime anime)
		=> FileUtils.GetRelativeOrAbsoluteFile(anime.GetDirectory(), anime.VideoInfo?.File);

	public static string GetSourceFile(this IAnime anime)
		=> FileUtils.EnsureAbsoluteFile(anime.GetDirectory(), anime.Source)!;

	public static string GetVideoFile(this ISong song, IAnime anime, int resolution)
		=> FileUtils.EnsureAbsoluteFile(anime.GetDirectory(), $"[{anime.Id}] {song.Name} [{resolution}p].webm")!;

	public static bool HasTimeStamp(this ISong song)
		=> song.Start > TimeSpan.FromSeconds(0);

	public static bool IsMissing(this ISong song, Status status)
		=> (song.Status & status) == 0;

	public static bool IsUnsubmitted(this ISong song)
		=> song.Status == Status.NotSubmitted;

	internal static FormatException InvalidFormat<T>(string? s)
		=> new($"Invalid {typeof(T).Name} format: {s}.");
}