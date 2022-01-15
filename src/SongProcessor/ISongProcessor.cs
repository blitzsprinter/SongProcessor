using SongProcessor.Models;

namespace SongProcessor;

public interface ISongProcessor
{
	string CreateFixes(IEnumerable<IAnime> anime);

	IReadOnlyList<ISongJob> CreateJobs(IEnumerable<IAnime> anime);
}