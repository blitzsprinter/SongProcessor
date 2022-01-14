using SongProcessor.Models;

namespace SongProcessor;

public interface ISongLoader
{
	string Extension { get; set; }

	Task<IAnime?> LoadAsync(string path);

	Task SaveAsync(string path, IAnimeBase anime);
}