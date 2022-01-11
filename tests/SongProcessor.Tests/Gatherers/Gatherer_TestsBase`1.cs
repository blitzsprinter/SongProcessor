using FluentAssertions;

using SongProcessor.Gatherers;
using SongProcessor.Models;

namespace SongProcessor.Tests.Gatherers;

public abstract class Gatherer_TestsBase
{
	public const string WEB_CALL_CATEGORY = "WEB_CALL";
}

public abstract class Gatherer_TestsBase<T> : Gatherer_TestsBase where T : IAnimeGatherer
{
	protected abstract IAnimeBase ExpectedAnimeBase { get; }
	protected abstract T Gatherer { get; }
	protected GatherOptions GatherOptions { get; } = new(
		AddEndings: true,
		AddInserts: true,
		AddOpenings: true,
		AddSongs: true
	);

	public virtual async Task AssertRetrievedMatchesAsync(int id)
	{
		var actual = await Gatherer.GetAsync(id, GatherOptions).ConfigureAwait(false);
		actual.Should().BeEquivalentTo(ExpectedAnimeBase);
	}
}