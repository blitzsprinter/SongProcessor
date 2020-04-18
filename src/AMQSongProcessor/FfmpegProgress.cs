using System;

namespace AMQSongProcessor
{
	public sealed class FfmpegProgress
	{
		public double Bitrate { get; internal set; }
		public int DroppedFrames { get; internal set; }
		public int DuplicateFrames { get; internal set; }
		public double Fps { get; internal set; }
		public int Frame { get; internal set; }
		public bool IsEnd { get; internal set; }
		public TimeSpan OutTime { get; internal set; }
		public long OutTimeMs { get; internal set; }
		public long OutTimeUs { get; internal set; }
		public double Speed { get; internal set; }
		public double Stream00q { get; internal set; }
		public long TotalSize { get; internal set; }
	}
}