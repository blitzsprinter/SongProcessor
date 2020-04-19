using System.Threading.Tasks;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Gatherers
{
	public interface IAnimeGatherer
	{
		string Name { get; }

		Task<Anime> GetAsync(int id, GatherOptions? options = null);
	}
}