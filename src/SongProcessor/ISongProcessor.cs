using SongProcessor.Models;

namespace SongProcessor;

public interface ISongProcessor
{
	IReadOnlyList<ISongJob> CreateJobs(IEnumerable<IAnime> anime);

	Task ExportFixesAsync(string dir, IEnumerable<IAnime> anime);
}