using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public interface ISongLoader
	{
		string Extension { get; set; }
		bool RemoveIgnoredSongs { get; set; }

		Task<Song> DuplicateSongAsync(Song song);

		Task<Anime> LoadAsync(string file);

		Task SaveAsync(Anime anime, SaveNewOptions? options = null);
	}

	public static class ISongLoaderUtils
	{
		public static async IAsyncEnumerable<Anime> LoadFromDirectoryAsync(this ISongLoader loader, string dir)
		{
			var pattern = $"*.{loader.Extension}";
			foreach (var file in Directory.EnumerateFiles(dir, pattern, SearchOption.AllDirectories))
			{
				yield return await loader.LoadAsync(file).CAF();
			}
		}
	}
}