using System.Collections.Generic;
using System.Threading.Tasks;

using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public interface ISongLoader
	{
		string Extension { get; set; }
		bool RemoveIgnoredSongs { get; set; }

		Task<Song> DuplicateSongAsync(Song song);

		IAsyncEnumerable<Anime> LoadAsync(string dir);

		Task<Anime> LoadFromANNAsync(int id, SaveNewOptions? options = null);

		Task SaveAsync(Anime anime);
	}
}