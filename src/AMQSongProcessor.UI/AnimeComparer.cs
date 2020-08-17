using System.Collections.Generic;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.UI
{
	public sealed class AnimeComparer : Comparer<IAnime>
	{
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

			return string.Compare(x.AbsoluteInfoPath, y.AbsoluteInfoPath);
		}
	}
}