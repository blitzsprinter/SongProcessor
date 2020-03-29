using System;

namespace AMQSongProcessor
{
	public sealed class FfmpegProgress
	{
		public double Bitrate { get; set; }
		public int DroppedFrames { get; set; }
		public int DuplicateFrames { get; set; }
		public double Fps { get; set; }
		public int Frame { get; set; }
		public bool IsEnd { get; set; }
		public TimeSpan OutTime { get; set; }
		public long OutTimeMs { get; set; }
		public long OutTimeUs { get; set; }
		public double Speed { get; set; }
		public double Stream00q { get; set; }
		public long TotalSize { get; set; }
	}
}