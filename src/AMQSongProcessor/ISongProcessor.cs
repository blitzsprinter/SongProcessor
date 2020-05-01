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

		IReadOnlyList<ISongJob> CreateJobs(IEnumerable<Anime> anime);

		Task ExportFixesAsync(string dir, IEnumerable<Anime> anime);
	}
}