using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public interface ISongProcessor
	{
		IProgress<ProcessingData> Processing { get; set; }
		IProgress<string> Warnings { get; set; }

		Task ExportFixesAsync(string dir, IReadOnlyList<Anime> anime);

		Task ProcessAsync(IReadOnlyList<Anime> anime);
	}
}