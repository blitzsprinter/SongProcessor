namespace SongProcessor.Models;

public sealed class AnimeComparer : Comparer<IAnime>
{
	public static AnimeComparer Instance { get; } = new();

	public override int Compare(IAnime? x, IAnime? y)
	{
		if (x is not null)
		{
			if (y is not null)
			{
				return CompareNonNull(x, y);
			}
			return 1;
		}
		return y is not null ? -1 : 0;
	}

	private static int CompareNonNull(IAnime x, IAnime y)
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
			song = SongComparer.Instance.Compare(x.Songs[i], y.Songs[i]);
		}
		if (song != 0)
		{
			return song;
		}

		return string.Compare(x.AbsoluteInfoPath, y.AbsoluteInfoPath);
	}
}