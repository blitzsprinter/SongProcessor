using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AMQSongProcessor.Jobs;
using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public interface ISongProcessor
	{
		event Action<string> WarningReceived;

		IReadOnlyList<ISongJob> CreateJobs(IEnumerable<IAnime> anime);

		Task ExportFixesAsync(string dir, IEnumerable<IAnime> anime);
	}
}