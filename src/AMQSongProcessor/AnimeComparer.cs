using System;
using System.Collections.Generic;

using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public sealed class AnimeComparer : Comparer<IAnime>
	{
		private readonly SongComparer _SongComparer = new();

		public override int Compare(IAnime? x, IAnime? y)
		{
			if (x != null)
			{
				if (y != null)
				{
					return CompareNonNull(x, y);
				}
				return 1;
			}
			return y != null ? -1 : 0;
		}

		private int CompareNonNull(IAnime x, IAnime y)
		{
			var year = x.Year.CompareTo(y.Year);
			if (year != 0)
			{
				return year;
			}

			var name = x.Name.CompareTo(y.Name);
			if (name != 0)
			{
				return name;
			}

			var song = 0;
			var count = Math.Min(x.Songs.Count, y.Songs.Count);
			for (var i = 0; i < count && song == 0; ++i)
			{
				song = _SongComparer.Compare(x.Songs[i], y.Songs[i]);
			}
			if (song != 0)
			{
				return song;
			}

			return string.Compare(x.AbsoluteInfoPath, y.AbsoluteInfoPath);
		}
	}
}