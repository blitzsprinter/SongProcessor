using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace AMQSongProcessor.Ffmpeg
{
	internal sealed class FfmpegProgressBuilder
	{
		public const string BITRATE = "bitrate";
		public const string DROP_FRAMES = "drop_frames";
		public const string DUP_FRAMES = "dup_frames";
		public const string FPS = "fps";
		public const string FRAME = "frame";
		public const string OUT_TIME = "out_time";
		public const string OUT_TIME_MS = "out_time_ms";
		public const string OUT_TIME_US = "out_time_us";
		public const string PROGRESS = "progress";
		public const string SPEED = "speed";
		public const string STREAM00Q = "stream_0_0_q";
		public const string TOTAL_SIZE = "total_size";

		public static ImmutableHashSet<string> ValidKeys = ImmutableHashSet.Create(new[]
		{
			BITRATE,
			DROP_FRAMES,
			DUP_FRAMES,
			FPS,
			FRAME,
			OUT_TIME,
			OUT_TIME_MS,
			OUT_TIME_US,
			PROGRESS,
			SPEED,
			STREAM00Q,
			TOTAL_SIZE,
		});

		private Dictionary<string, string> _Values = new();

		public bool IsNextProgressReady(string kvp, [NotNullWhen(true)] out FfmpegProgress? progress)
		{
			var split = kvp.Split('=');
			var key = split[0].Trim();
			if (!ValidKeys.Contains(key))
			{
				throw new ArgumentException($"Invalid key provided: {key}.", nameof(kvp));
			}

			_Values[key] = split[1].Trim();

			// progress will be the last key in the kvp collection
			if (key != PROGRESS)
			{
				progress = null;
				return false;
			}

			var values = _Values;
			_Values = new();

			progress = new
			(
				Bitrate: Parse(values, BITRATE, x => double.Parse(x.Replace("kbits/s", ""))),
				DroppedFrames: Parse(values, DROP_FRAMES, int.Parse),
				DuplicateFrames: Parse(values, DUP_FRAMES, int.Parse),
				Fps: Parse(values, FPS, double.Parse),
				Frame: Parse(values, FRAME, int.Parse),
				IsEnd: values[PROGRESS] == "end",
				OutTime: Parse(values, OUT_TIME, TimeSpan.Parse),
				OutTimeMs: Parse(values, OUT_TIME_MS, long.Parse),
				OutTimeUs: Parse(values, OUT_TIME_US, long.Parse),
				Speed: Parse(values, SPEED, x => double.Parse(x.Replace("x", ""))),
				Stream00q: Parse(values, STREAM00Q, double.Parse),
				TotalSize: Parse(values, TOTAL_SIZE, long.Parse)
			);
			return true;
		}

		private static T Parse<T>(
			Dictionary<string, string> source,
			string name,
			Func<string, T> parser,
			T missingVal = default) where T : struct
		{
			if (source.TryGetValue(name, out var value))
			{
				if (value == "N/A")
				{
					return default;
				}

				return parser(value);
			}
			return missingVal;
		}
	}
}