using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Models;

namespace SongProcessor.Tests.Models;

[TestClass]
public sealed class AnimeComparer_Tests
{
	private static Anime Anime { get; } = new Anime(@"C:\anime.amq", new AnimeBase
	{
		Id = 73,
		Name = "Anime",
		Year = 1984,
	}, null);

	[TestMethod]
	public void Ordering_Test()
	{
		var expected = new[]
		{
			null,
			null,
			Copy(x => x.Year = Anime.Year - 1),
			Copy(x => x.Name = x.Name[0..^2]),
			Copy(_ => { }, x => x[0..^2]),
			Anime,
			Copy(x => x.Songs.Add(new()
			{
				Type = SongType.Op.Create(1),
			})),
			Copy(x => x.Songs.Add(new()
			{
				Type = SongType.Op.Create(1),
			})),
			Copy(x => x.Songs.Add(new()
			{
				Type = SongType.Ed.Create(1),
			})),
			Copy(_ => { }, x => "ZZZ" + x),
			Copy(x => x.Name += "a"),
			Copy(x => x.Year = Anime.Year + 1),
		};

		var rng = new Random(0);
		var randomized = expected.OrderBy(_ => rng.Next()).ToList();
		randomized.Should().NotBeInAscendingOrder(AnimeComparer.Instance);

		var actual = new SortedSet<Anime?>(randomized, AnimeComparer.Instance);
		actual.Should().BeInAscendingOrder(AnimeComparer.Instance);
	}

	private static Anime Copy(
		Action<AnimeBase> modify,
		Func<string, string>? modifyFileName = null)
	{
		var animeBase = new AnimeBase(Anime);
		modify(animeBase);
		var absoluteInfoPath = Anime.AbsoluteInfoPath;
		if (modifyFileName is not null)
		{
			var dir = Path.GetDirectoryName(absoluteInfoPath);
			var ext = Path.GetExtension(absoluteInfoPath);
			var old = Path.GetFileNameWithoutExtension(absoluteInfoPath);
			var @new = modifyFileName(old);
			absoluteInfoPath = Path.Combine(dir!, @new + ext);
		}
		return new(absoluteInfoPath, animeBase, Anime.VideoInfo);
	}
}