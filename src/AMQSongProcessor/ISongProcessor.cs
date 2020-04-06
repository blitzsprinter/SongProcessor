using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AMQSongProcessor.Jobs;
using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public interface ISongProcessor
	{
		IProgress<ProcessingData>? Processing { get; set; }
		IProgress<string>? Warnings { get; set; }

		IReadOnlyList<ISongJob> CreateJobs(IEnumerable<Anime> anime);

		Task ExportFixesAsync(string dir, IEnumerable<Anime> anime);

		Task ProcessAsync(IEnumerable<ISongJob> jobs, CancellationToken? token = null);
	}
}