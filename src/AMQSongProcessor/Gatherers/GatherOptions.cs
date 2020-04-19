using System;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Gatherers
{
	public sealed class GatherOptions
	{
		public bool AddEndings { get; set; }
		public bool AddInserts { get; set; }
		public bool AddOpenings { get; set; }
		public bool AddSongs { get; set; }

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