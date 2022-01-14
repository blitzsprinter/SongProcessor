using SongProcessor.Models;

namespace SongProcessor;

public interface ISongLoader
{
	string Extension { get; set; }

	Task<IAnime?> LoadAsync(string file);

	Task SaveAsync(string file, IAnimeBase anime);
}