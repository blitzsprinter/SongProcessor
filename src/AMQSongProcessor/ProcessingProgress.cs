using System;

namespace AMQSongProcessor
{
	public sealed class ProcessingProgress : IProgress<ProcessingData>
	{
		private string _CurrentlyProcessing;

		public void Report(ProcessingData value)
		{
			if (_CurrentlyProcessing != value.Path)
			{
				_CurrentlyProcessing = value.Path;
				Console.WriteLine();
			}

			if (value.Percentage == 1)
			{
				Console.WriteLine($"Finished processing \"{value.Path}\".\n");
				return;
			}
			Console.WriteLine($"\"{value.Path}\" is {value.Percentage * 100:00.0}% complete.");
		}
	}
}