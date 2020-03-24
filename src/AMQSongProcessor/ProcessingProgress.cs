using System;
using System.Threading;

namespace AMQSongProcessor
{
	public sealed class ProcessingProgress : IProgress<ProcessingData>
	{
		private string _Current;

		public void Report(ProcessingData value)
		{
			//For each new path, add in an extra line break for readability
			if (Interlocked.Exchange(ref _Current, value.Path) != value.Path)
			{
				Console.WriteLine();
			}

			if (value.Percentage == 1)
			{
				Console.WriteLine($"Finished processing \"{value.Path}\"\n");
				return;
			}
			Console.WriteLine($"\"{value.Path}\" is {value.Percentage * 100:00.0}% complete. " +
				$"ETA on completion: {value.CompletionETA}");
		}
	}
}