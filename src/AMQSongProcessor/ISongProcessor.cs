using System.Collections.Generic;
using System.Threading.Tasks;

using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public interface ISongProcessor
	{
		Task ExportFixesAsync(string dir, IReadOnlyList<Anime> anime);

		IAsyncEnumerable<string> ProcessAsync(IReadOnlyList<Anime> anime);
	}
}