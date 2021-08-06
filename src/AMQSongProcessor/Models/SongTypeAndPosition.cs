using System;
using System.Diagnostics;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
	public readonly struct SongTypeAndPosition : IEquatable<SongTypeAndPosition>, IComparable<SongTypeAndPosition>
	{
		public string LongType => Type switch
		{
			SongType.Op => nameof(SongType.Opening),
			SongType.Ed => nameof(SongType.Ending),
			SongType.In => nameof(SongType.Insert),
			_ => throw new ArgumentOutOfRangeException(nameof(Type)),
		};
		public int? Position { get; }
		public string ShortType => Type switch
		{
			SongType.Op => nameof(SongType.Op),
			SongType.Ed => nameof(SongType.Ed),
			SongType.In => nameof(SongType.In),
			_ => throw new ArgumentOutOfRangeException(nameof(Type)),
		};
		public SongType Type { get; }
		private string DebuggerDisplay => ToString();

		public SongTypeAndPosition(SongType type, int? position)
		{
			if (!Enum.IsDefined(typeof(SongType), type))
			{
				throw new ArgumentOutOfRangeException(nameof(type), "Invalid song type.");
			}

			Type = type;
			Position = position;
		}

		public static bool operator !=(SongTypeAndPosition item1, SongTypeAndPosition item2)
			=> !(item1 == item2);

		public static bool operator ==(SongTypeAndPosition item1, SongTypeAndPosition item2)
			=> item1.Equals(item2);

		public static SongTypeAndPosition Parse(string? s)
		{
			if (!TryParse(s, out var result))
			{
				throw new FormatException($"Invalid format: {s}");
			}
			return result;
		}

		public static bool TryParse(string? s, out SongTypeAndPosition result)
		{
			if (s == null)
			{
				result = default;
				return false;
			}

			var index = GetFirstDigitIndex(s);
			if (!Enum.TryParse<SongType>(s[..index].Trim(), out var type))
			{
				result = default;
				return false;
			}

			var position = default(int?);
			if (index != s.Length)
			{
				if (!int.TryParse(s[index..], out var parsed) || parsed < 1)
				{
					result = default;
					return false;
				}

				position = parsed;
			}

			result = new(type, position);
			return true;
		}

		public int CompareTo(SongTypeAndPosition other)
		{
			var typeComparison = Type.CompareTo(other.Type);
			if (typeComparison != 0)
			{
				return typeComparison;
			}
			return Nullable.Compare(Position, other.Position);
		}

		public override bool Equals(object? obj)
			=> Equals(obj as SongTypeAndPosition?);

		public bool Equals(SongTypeAndPosition? other)
		{
			if (other == null)
			{
				return false;
			}
			return Equals(other.Value);
		}

		public bool Equals(SongTypeAndPosition other)
			=> Type == other.Type && Position == other.Position;

		public override int GetHashCode()
			=> HashCode.Combine(Type, Position);

		public override string ToString()
		{
			var s = LongType;
			if (Type == SongType.In || Position == null)
			{
				return s;
			}

			return s + " " + Position.ToString();
		}

		private static int GetFirstDigitIndex(string value)
		{
			for (var i = 0; i < value.Length; ++i)
			{
				if (char.IsDigit(value[i]))
				{
					return i;
				}
			}
			return value.Length;
		}
	}
}