using System;
using System.Diagnostics;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public readonly struct VolumeModifer
	{
		private const string DB = "dB";

		public int? Decibels { get; }
		public double? Percentage { get; }
		private string DebuggerDisplay => ToString();

		private VolumeModifer(double? percentage, int? dbs)
		{
			Percentage = percentage;
			Decibels = dbs;
		}

		public static VolumeModifer FromDecibels(int dbs)
			=> new VolumeModifer(null, dbs);

		public static VolumeModifer FromPercentage(double percentage)
			=> new VolumeModifer(percentage, null);

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
			if (s == null)
			{
				result = default;
				return false;
			}

			if (double.TryParse(s, out var percentage))
			{
				result = FromPercentage(percentage);
				return true;
			}
			else if (int.TryParse(s.Replace(DB, null), out var dbs))
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
			return Percentage.ToString();
		}
	}
}