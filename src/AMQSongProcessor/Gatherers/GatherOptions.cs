using System;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Gatherers
{
	public sealed record GatherOptions(
		bool AddEndings,
		bool AddInserts,
		bool AddOpenings,
		bool AddSongs
	)
	{
		public bool CanBeGathered(SongType type)
		{
			return AddSongs && type switch
			{
				SongType.Ed => AddEndings,
				SongType.In => AddInserts,
				SongType.Op => AddOpenings,
				_ => throw new ArgumentOutOfRangeException(nameof(type)),
			};
		}
	}
}