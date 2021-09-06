using AMQSongProcessor.Gatherers;
using AMQSongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.Gatherers
{
	public abstract class Gatherer_TestsBase<T> where T : IAnimeGatherer
	{
		public abstract T Gatherer { get; }

		public virtual async Task AssertRetrievedMatchesAsync(int id)
		{
			var actual = await Gatherer.GetAsync(id, new(
				AddEndings: true,
				AddInserts: true,
				AddOpenings: true,
				AddSongs: true
			)).ConfigureAwait(false);

			var expected = GetExpectedAnimeBase();
			Assert.AreEqual(expected.Id, actual.Id);
			Assert.AreEqual(expected.Name, actual.Name);
			Assert.AreEqual(expected.Source, actual.Source);
			Assert.AreEqual(expected.Year, actual.Year);

			Assert.AreEqual(expected.Songs.Count, actual.Songs.Count);
			for (var i = 0; i < expected.Songs.Count; ++i)
			{
				Assert.AreEqual(expected.Songs[i].Artist, actual.Songs[i].Artist);
				Assert.AreEqual(expected.Songs[i].Name, actual.Songs[i].Name);
				Assert.AreEqual(expected.Songs[i].Type, actual.Songs[i].Type);
			}
		}

		public abstract IAnimeBase GetExpectedAnimeBase();
	}
}