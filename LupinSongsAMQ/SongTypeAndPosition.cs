using System;

namespace LupinSongsAMQ
{
	public struct SongTypeAndPosition
	{
		private readonly SongType _Type;

		public string LongType => _Type switch
		{
			SongType.Op => nameof(SongType.Opening),
			SongType.Ed => nameof(SongType.Ending),
			SongType.In => nameof(SongType.Insert),
			_ => throw new ArgumentOutOfRangeException(nameof(_Type)),
		};

		public int? Position { get; }

		public string ShortType => _Type switch
		{
			SongType.Op => nameof(SongType.Op),
			SongType.Ed => nameof(SongType.Ed),
			SongType.In => nameof(SongType.In),
			_ => throw new ArgumentOutOfRangeException(nameof(_Type)),
		};

		public SongTypeAndPosition(SongType type, int? position)
		{
			if (!Enum.IsDefined(typeof(SongType), type))
			{
				throw new ArgumentOutOfRangeException(nameof(type), "Invalid song type.");
			}

			_Type = type;
			Position = position;
		}

		public override string ToString()
		{
			var s = LongType;
			if (_Type == SongType.In || Position == null)
			{
				return s;
			}

			return s + " " + Position.ToString();
		}
	}
}