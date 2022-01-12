using FluentAssertions;

using SongProcessor.Gatherers;
using SongProcessor.Models;

using System.Net;

namespace SongProcessor.Tests.Gatherers;

public abstract class Gatherer_TestsBase
{
	public const string WEB_REQUEST = "Web_Request";
}

public abstract class Gatherer_TestsBase<T> : Gatherer_TestsBase where T : IAnimeGatherer
{
	protected abstract IAnimeBase ExpectedAnimeBase { get; }
	protected abstract T Gatherer { get; set; }
	protected GatherOptions GatherOptions { get; set; } = GatherOptions.All;

	public virtual async Task AssertRetrievedMatchesAsync(int id)
	{
		var castedGatherer = (IAnimeGatherer)Gatherer;
		var actual = await castedGatherer.GetAsync(id, GatherOptions).ConfigureAwait(false);
		actual.Should().BeEquivalentTo(ExpectedAnimeBase);
	}

	protected class HttpTestHandler : HttpMessageHandler
	{
		public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
			=> Task.FromResult<HttpResponseMessage>(new(StatusCode));
	}
}