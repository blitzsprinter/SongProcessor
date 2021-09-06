using AMQSongProcessor.Gatherers;
using AMQSongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.Gatherers
{
	[TestClass]
	public sealed class ANNGatherer_Tests : Gatherer_TestsBase<ANNGatherer>
	{
		public override ANNGatherer Gatherer { get; } = new();

		[TestMethod]
		public async Task Gather_Test()
			=> await AssertRetrievedMatchesAsync(13888).ConfigureAwait(false);

		public override IAnimeBase GetExpectedAnimeBase()
		{
			return new AnimeBase
			{
				Id = 13888,
				Name = "Jormungand",
				Songs = new()
				{
					new()
					{
						Artist = "Mami Kawada",
						Name = "Borderland",
						Type = SongType.Op.Create(null),
					},
					new()
					{
						Artist = "Nagi Yanagi",
						Name = "Ambivalentidea",
						Type = SongType.Ed.Create(1),
					},
					new()
					{
						Artist = "Nagi Yanagi",
						Name = "Shiroku Yawaraka na Hana",
						Type = SongType.Ed.Create(2),
					},
					// ANN doesn't have the inserts documented for this
				},
				Source = null,
				Year = 2012
			};
		}
	}
}