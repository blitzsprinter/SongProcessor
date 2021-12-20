using SongProcessor.Models;

namespace SongProcessor.Results;

public class Canceled : Result
{
	public static Canceled Instance { get; } = new();

	// IsSuccess = null because cancellation isn't really an error
	public Canceled() : base("Process was canceled.", null)
	{
	}
}

public class Error : Result
{
	public int Code { get; }
	public IReadOnlyList<string> Errors { get; }

	public Error(int code, IReadOnlyList<string> errors)
		: base($"Encountered an error ({code}): {string.Join('\n', errors)}.", false)
	{
		Code = code;
		Errors = errors;
	}
}

public class FileAlreadyExists : Result
{
	public string Path { get; }

	public FileAlreadyExists(string path) : base($"{path} already exists.", false)
	{
		Path = path;
	}
}

public sealed class IsIgnored : Result
{
	public ISong Song { get; }

	public IsIgnored(ISong song) : base($"Is ignored: {song.Name}", false)
	{
		Song = song;
	}
}

public class SourceIsNull : Result
{
	public IAnime Anime { get; }

	public SourceIsNull(IAnime anime) : base($"Source is null: {anime.Name}", false)
	{
		Anime = anime;
	}
}

public class Success : Result
{
	public static Success Instance { get; } = new();

	public Success() : base("Successfully encoded the output.", true)
	{
	}
}

public sealed class TimestampIsNull : Result
{
	public ISong Song { get; }

	public TimestampIsNull(ISong song) : base($"Timestamp is null: {song.Name}", false)
	{
		Song = song;
	}
}

public sealed class VideoIsNull : Result
{
	public IAnime Anime { get; }

	public VideoIsNull(IAnime anime) : base($"Video info is null: {anime.Name}", false)
	{
		Anime = anime;
	}
}

public class VideoTooSmall : Result
{
	public IAnime Anime { get; }
	public int Resolution { get; }

	public VideoTooSmall(IAnime anime, int resolution)
		: base($"Source is smaller than {resolution}p: {anime.Name}", false)
	{
		Anime = anime;
		Resolution = resolution;
	}
}