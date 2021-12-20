using System.Diagnostics;

namespace SongProcessor.Models;

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public readonly struct AspectRatio : IEquatable<AspectRatio>, IComparable<AspectRatio>
{
	public const char SEPARATOR = '/';
	public static AspectRatio Square { get; } = new(1, 1);

	public int Height { get; }
	public float Ratio => Width / (float)Height;
	public int Width { get; }
	private string DebuggerDisplay => ToString();

	public AspectRatio(int width, int height)
	{
		if (width < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(width));
		}
		if (height < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(height));
		}

		Width = width;
		Height = height;
	}

	public static bool operator !=(AspectRatio item1, AspectRatio item2)
		=> !item1.Equals(item2);

	public static bool operator ==(AspectRatio item1, AspectRatio item2)
		=> item1.Equals(item2);

	public static AspectRatio Parse(string s, char separator)
	{
		if (!TryParse(s, separator, out var result))
		{
			throw ModelUtils.InvalidFormat<AspectRatio>(s);
		}
		return result;
	}

	public static bool TryParse(string s, char separator, out AspectRatio result)
	{
		if (s is null)
		{
			result = default;
			return false;
		}

		var values = s.Split(separator);
		if (values.Length != 2
			|| !int.TryParse(values[0], out var width)
			|| !int.TryParse(values[1], out var height))
		{
			result = default;
			return false;
		}

		result = new(width, height);
		return true;
	}

	public int CompareTo(AspectRatio other)
		=> Ratio.CompareTo(other.Ratio);

	public override bool Equals(object? obj)
		=> Equals(obj as AspectRatio?);

	public bool Equals(AspectRatio? other)
		=> other is not null && Equals(other.Value);

	public bool Equals(AspectRatio other)
		=> Height == other.Height && Width == other.Width;

	public override int GetHashCode()
		=> HashCode.Combine(Width, Height);

	public override string ToString()
		=> ToString(SEPARATOR);

	public string ToString(char separator)
		=> Width.ToString() + separator + Height.ToString();
}