namespace SongProcessor.Models;

public interface IAnimeBase
{
	public int Id { get; }
	public string Name { get; }
	public IReadOnlyList<ISong> Songs { get; }
	public string? Source { get; }
	public int Year { get; }
}
