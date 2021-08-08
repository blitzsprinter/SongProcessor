using System;

namespace AMQSongProcessor
{
	public sealed class ProcessingData
	{
		public TimeSpan CompletionETA { get; }
		public string File { get; }
		public TimeSpan Length { get; }
		public string Path { get; }
		public float Percentage { get; }
		public FfmpegProgress Progress { get; }
		public TimeSpan Remaining { get; }

		public ProcessingData(string path, TimeSpan length, FfmpegProgress progress)
		{
			Path = path;
			File = System.IO.Path.GetFileName(Path);
			Length = length;
			Progress = progress;

			Percentage = Math.Min(1f, Progress.OutTime.Ticks / (float)Length.Ticks);
			Remaining = TimeSpan.FromTicks(Length.Ticks - Progress.OutTime.Ticks);
			var compTicks = Math.Max(0, (long)(Remaining.Ticks / Progress.Speed));
			CompletionETA = TimeSpan.FromTicks(compTicks);
		}
	}
}