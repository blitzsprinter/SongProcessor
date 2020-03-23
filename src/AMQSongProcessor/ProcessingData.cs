using System;

namespace AMQSongProcessor
{
	public readonly struct ProcessingData
	{
		public double Bitrate { get; }
		public TimeSpan CompletionETA { get; }
		public TimeSpan Length { get; }
		public string Path { get; }
		public double Percentage { get; }
		public TimeSpan Remaining { get; }
		public int Size { get; }
		public double Speed { get; }
		public TimeSpan Time { get; }

		public ProcessingData(string path, int size, TimeSpan time, double bitrate, double speed, double progress)
		{
			Path = path;
			Size = size;
			Time = time;
			Bitrate = bitrate;
			Speed = speed;
			Percentage = progress;

			Length = TimeSpan.FromTicks((long)(Time.Ticks / Percentage));
			Remaining = TimeSpan.FromTicks(Length.Ticks - Time.Ticks);
			CompletionETA = TimeSpan.FromTicks((long)(Remaining.Ticks / Speed));
		}
	}
}