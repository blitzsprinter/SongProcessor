using System;
using System.Threading;

namespace AMQSongProcessor
{
	internal sealed class LogProcessingToConsole : IProgress<ProcessingData>
	{
		private string? _Current;

		public void Report(ProcessingData value)
		{
			//For each new path, add in an extra line break for readability
			var firstWrite = Interlocked.Exchange(ref _Current, value.Path) != value.Path;
			var finalWrite = value.Progress.IsEnd;
			if (firstWrite || finalWrite)
			{
				Console.WriteLine();
			}

			if (finalWrite)
			{
				Console.WriteLine($"Finished processing \"{value.Path}\"\n");
				return;
			}

			if (!firstWrite)
			{
				Console.CursorLeft = 0;
			}

			Console.Write($"\"{value.Path}\" is {value.Percentage * 100:00.0}% complete. " +
				$"ETA on completion: {value.CompletionETA}");
		}
	}
}