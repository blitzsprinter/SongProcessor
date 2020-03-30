using System;

namespace AMQSongProcessor
{
	public readonly struct ProcessingData
	{
		public TimeSpan CompletionETA { get; }
		public string File => System.IO.Path.GetFileName(Path);
		public TimeSpan Length { get; }
		public string Path { get; }
		public double Percentage { get; }
		public FfmpegProgress Progress { get; }
		public TimeSpan Remaining { get; }

		public ProcessingData(string path, TimeSpan length, FfmpegProgress progress)
		{
			Path = path;
			Length = length;
			Progress = progress;

			Percentage = Progress.OutTime.Ticks / (double)Length.Ticks;
			Remaining = TimeSpan.FromTicks(Length.Ticks - Progress.OutTime.Ticks);
			var compTicks = Math.Max(0, (long)(Remaining.Ticks / Progress.Speed));
			CompletionETA = TimeSpan.FromTicks(compTicks);
		}
	}
}