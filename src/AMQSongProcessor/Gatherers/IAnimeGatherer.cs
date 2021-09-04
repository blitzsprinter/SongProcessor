
using AMQSongProcessor.Models;

namespace AMQSongProcessor.Gatherers
{
	public interface IAnimeGatherer
	{
		string Name { get; }

		Task<IAnimeBase> GetAsync(int id, GatherOptions? options = null);
	}
}