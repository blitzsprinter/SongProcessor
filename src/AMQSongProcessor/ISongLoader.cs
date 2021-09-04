
using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public interface ISongLoader
	{
		string Extension { get; set; }

		Task<IAnime?> LoadAsync(string file);

		Task<string?> SaveAsync(string directory, IAnimeBase anime, SaveNewOptions? options = null);
	}
}