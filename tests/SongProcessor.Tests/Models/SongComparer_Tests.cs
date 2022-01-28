using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Models;

namespace SongProcessor.Tests.Models;

[TestClass]
public sealed class SongComparer_Tests
{
	private static Song Song { get; } = new()
	{
		Name = "Name",
		Artist = "Artist",
		Type = SongType.Ed.Create(2),
	};

	[TestMethod]
	public void Ordering_Test()
	{
		var expected = new[]
		{
			null,
			null,
			Copy(x => x.Type = SongType.Op.Create(1)),
			Copy(x => x.Type = SongType.Ed.Create(1)),
			Song,
			Copy(x => x.Name = "joe"),
			Copy(x => x.Episode = 1),
			Copy(x => x.Episode = 2),
			Copy(x => x.Episode = 3),
			Copy(x =>
			{
				x.Episode = 3;
				x.Start = TimeSpan.FromMinutes(1);
			}),
			Copy(x =>
			{
				x.Episode = 3;
				x.Start = TimeSpan.FromMinutes(2);
			}),
			Copy(x =>
			{
				x.Episode = 3;
				x.Start = TimeSpan.FromMinutes(3);
			}),
			Copy(x =>
			{
				x.Episode = 3;
				x.Start = TimeSpan.FromMinutes(3);
			}),
			Copy(x => x.Type = SongType.Ed.Create(3)),
			Copy(x => x.Type = SongType.In.Create(null)),
		};
		expected.Should().BeInAscendingOrder(SongComparer.Instance);

		var rng = new Random(0);
		var randomized = expected.OrderBy(_ => rng.Next()).ToList();
		randomized.Should().NotBeInAscendingOrder(SongComparer.Instance);

		var actual = new SortedSet<Song?>(randomized, SongComparer.Instance);
		actual.Should().BeInAscendingOrder(SongComparer.Instance);
	}

	private static Song Copy(Action<Song> modify)
	{
		var song = new Song(Song);
		modify(song);
		return song;
	}
}