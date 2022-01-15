using System.Diagnostics;

namespace SongProcessor.Models;

[DebuggerDisplay(ModelUtils.DEBUGGER_DISPLAY)]
public readonly struct VolumeModifer
{
	public const string DB = "dB";

	public VolumeModifierType Type { get; }
	public double Value { get; }
	private string DebuggerDisplay => ToString();

	private VolumeModifer(VolumeModifierType type, double value)
	{
		if (type == VolumeModifierType.Percentage && value < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		Type = type;
		Value = value;
	}

	public static VolumeModifer FromDecibels(double dbs)
		=> new(VolumeModifierType.Decibels, dbs);

	public static VolumeModifer FromPercentage(double percentage)
		=> new(VolumeModifierType.Percentage, percentage);

	public static VolumeModifer Parse(string s)
	{
		if (!TryParse(s, out var result))
		{
			throw ModelUtils.InvalidFormat<VolumeModifer>(s);
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

		var span = s.AsSpan().Trim();
		if (double.TryParse(span, out var percentage))
		{
			result = FromPercentage(percentage);
			return true;
		}
		else if (span.EndsWith(DB) && double.TryParse(s[..(span.Length - 2)], out var dbs))
		{
			result = FromDecibels(dbs);
			return true;
		}

		result = default;
		return false;
	}

	public override string ToString()
	{
		if (Type == VolumeModifierType.Decibels)
		{
			return Value + DB;
		}
		return Value.ToString();
	}
}