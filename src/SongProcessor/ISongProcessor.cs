using SongProcessor.Models;

namespace SongProcessor;

public interface ISongProcessor
{
	IReadOnlyList<ISongJob> CreateJobs(IEnumerable<IAnime> anime);

	string ExportFixes(IEnumerable<IAnime> anime);
}