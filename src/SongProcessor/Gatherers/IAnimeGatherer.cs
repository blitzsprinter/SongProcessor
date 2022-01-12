using SongProcessor.Models;

namespace SongProcessor.Gatherers;

public interface IAnimeGatherer
{
	string Name { get; }

	Task<IAnimeBase> GetAsync(int id, GatherOptions options);
}