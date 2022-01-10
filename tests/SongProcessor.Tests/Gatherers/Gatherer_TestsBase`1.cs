using FluentAssertions;

using SongProcessor.Gatherers;
using SongProcessor.Models;

namespace SongProcessor.Tests.Gatherers;

public abstract class Gatherer_TestsBase
{
	public const string GATHERERS_CATEGORY = "HTML";
}

public abstract class Gatherer_TestsBase<T> : Gatherer_TestsBase where T : IAnimeGatherer
{
	public abstract T Gatherer { get; }

	public virtual async Task AssertRetrievedMatchesAsync(int id)
	{
		var expected = GetExpectedAnimeBase();
		var actual = await Gatherer.GetAsync(id, new(
			AddEndings: true,
			AddInserts: true,
			AddOpenings: true,
			AddSongs: true
		)).ConfigureAwait(false);

		actual.Should().BeEquivalentTo(expected);
	}

	public abstract IAnimeBase GetExpectedAnimeBase();
}