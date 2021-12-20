using SongProcessor.FFmpeg;

using System.Diagnostics;

namespace SongProcessor.Models;

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public class Anime : IAnime
{
	public string AbsoluteInfoPath { get; }
	public int Id { get; }
	public string Name { get; }
	public List<Song> Songs { get; }
	public string? Source => this.GetRelativeOrAbsoluteSourcePath();
	public SourceInfo<VideoInfo>? VideoInfo { get; }
	public int Year { get; }
	IReadOnlyList<ISong> IAnimeBase.Songs => Songs;
	private string DebuggerDisplay => Name;

	public Anime(string path, IAnimeBase other, SourceInfo<VideoInfo>? videoInfo)
	{
		if (path is null)
		{
			throw new ArgumentNullException(nameof(path));
		}
		if (Path.GetDirectoryName(path) is null)
		{
			throw new ArgumentException("Must be an absolute path.", nameof(path));
		}

		AbsoluteInfoPath = path;
		Id = other.Id;
		Name = other.Name;
		Songs = other.Songs?.Select(x => new Song(x))?.ToList() ?? new List<Song>();
		Year = other.Year;
		VideoInfo = videoInfo;
	}

	public Anime(IAnime anime) : this(anime.AbsoluteInfoPath, anime, anime.VideoInfo)
	{
	}
}