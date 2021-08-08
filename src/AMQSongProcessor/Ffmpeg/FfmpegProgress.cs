using System;

namespace AMQSongProcessor.Ffmpeg
{
	public sealed record FfmpegProgress(
		double Bitrate,
		int DroppedFrames,
		int DuplicateFrames,
		double Fps,
		int Frame,
		bool IsEnd,
		TimeSpan OutTime,
		long OutTimeMs,
		long OutTimeUs,
		double Speed,
		double Stream00q,
		long TotalSize
	);
}