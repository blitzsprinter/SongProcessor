﻿using System;
using System.Diagnostics;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
	public readonly struct VolumeModifer
	{
		private const string DB = "dB";

		public double? Decibels { get; }
		public double? Percentage { get; }
		private string DebuggerDisplay => ToString();

		private VolumeModifer(double? percentage, double? dbs)
		{
			Percentage = percentage;
			Decibels = dbs;
		}

		public static VolumeModifer FromDecibels(double dbs)
			=> new(null, dbs);

		public static VolumeModifer FromPercentage(double percentage)
			=> new(percentage, null);

		public static VolumeModifer Parse(string s)
		{
			if (!TryParse(s, out var result))
			{
				throw new FormatException($"Invalid format: {s}");
			}
			return result;
		}

		public static bool TryParse(string s, out VolumeModifer result)
		{
			if (s is null)
			{
				result = default;
				return false;
			}

			if (double.TryParse(s.Trim(), out var percentage))
			{
				result = FromPercentage(percentage);
				return true;
			}
			else if (double.TryParse(s.Replace(DB, null).Trim(), out var dbs))
			{
				result = FromDecibels(dbs);
				return true;
			}

			result = default;
			return false;
		}

		public override string ToString()
		{
			if (Decibels != null)
			{
				return Decibels + DB;
			}
			return (Percentage ?? 1).ToString();
		}
	}
}