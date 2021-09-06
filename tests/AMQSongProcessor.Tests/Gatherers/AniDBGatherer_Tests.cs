using AMQSongProcessor.Gatherers;
using AMQSongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.Gatherers
{
	[TestClass]
	[TestCategory("HTML")]
	public sealed class AniDBGatherer_Tests : Gatherer_TestsBase<AniDBGatherer>
	{
		public override AniDBGatherer Gatherer { get; } = new();

		[TestMethod]
		public async Task Gather_Test()
			=> await AssertRetrievedMatchesAsync(8842).ConfigureAwait(false);

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
						Artist = "Kawada Mami",
						Name = "Borderland",
						Type = SongType.Op.Create(1),
					},
					new()
					{
						Artist = "Yanagi Nagi",
						Name = "Ambivalentidea",
						Type = SongType.Ed.Create(1),
					},
					new()
					{
						Artist = "Yanagi Nagi",
						Name = "Shiroku Yawaraka na Hana",
						Type = SongType.Ed.Create(2),
					},
					new()
					{
						Artist = "SANTA",
						Name = "Time to Rock and Roll",
						Type = SongType.In.Create(null),
					},
					new()
					{
						Artist = "Giacomo Puccini",
						Name = "Tosca-Vissi D\u0060Arte, Vissi D\u0060Amore",
						Type = SongType.In.Create(null),
					},
					new()
					{
						Artist = "SANTA",
						Name = "Time to Attack",
						Type = SongType.In.Create(null),
					},
					new()
					{
						Artist = "Silvio Anastacio",
						Name = "Meu Mundo Amor",
						Type = SongType.In.Create(null),
					},
					new()
					{
						Artist = "Fukuoka Yutaka",
						Name = "Jormungand",
						Type = SongType.In.Create(null),
					},
				},
				Source = null,
				Year = 2012
			};
		}
	}
}