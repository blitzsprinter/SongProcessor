using System.Diagnostics;

namespace SongProcessor.Models;

[DebuggerDisplay(ModelUtils.DEBUGGER_DISPLAY)]
public class AnimeBase : IAnimeBase
{
	public int Id { get; set; }
	public string Name { get; set; }
	public List<Song> Songs { get; set; }
	public string? Source { get; set; }
	public int Year { get; set; }
	IReadOnlyList<ISong> IAnimeBase.Songs => Songs;
	private string DebuggerDisplay => Name;

	public AnimeBase()
	{
		Name = null!;
		Songs = [];
	}

	public AnimeBase(IAnimeBase other)
	{
		Id = other.Id;
		Name = other.Name;
		Songs = other.Songs.Select(x => new Song(x)).ToList();
		Source = other.Source;
		Year = other.Year;
	}
}