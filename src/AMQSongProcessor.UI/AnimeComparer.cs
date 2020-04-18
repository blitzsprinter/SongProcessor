using System.Collections.Generic;
using System.IO;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.UI
{
	public sealed class AnimeComparer : Comparer<Anime>
	{
		public override int Compare(Anime? x, Anime? y)
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

		private int CompareNonNull(Anime x, Anime y)
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

			var xCreated = File.GetCreationTime(x.AbsoluteInfoPath);
			var yCreated = File.GetCreationTime(y.AbsoluteInfoPath);
			return xCreated.CompareTo(yCreated);
		}
	}
}