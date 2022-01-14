namespace SongProcessor.Models;

public interface IAnimeBase
{
	int Id { get; }
	string Name { get; }
	IReadOnlyList<ISong> Songs { get; }
	string? Source { get; }
	int Year { get; }
}