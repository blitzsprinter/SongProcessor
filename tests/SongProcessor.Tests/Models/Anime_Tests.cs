using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Models;

namespace SongProcessor.Tests.Models;

[TestClass]
public sealed class Anime_Tests
{
	[TestMethod]
	public void ConstructorNonAbsolutePath_Test()
	{
		Action ctor = () => _ = new Anime(@"\anime.amq", new AnimeBase(), null);
		ctor.Should().Throw<ArgumentException>();
	}

	[TestMethod]
	public void ConstructorNullPath_Test()
	{
		Action ctor = () => _ = new Anime(null!, new AnimeBase(), null);
		ctor.Should().Throw<ArgumentNullException>();
	}

	[TestMethod]
	public void CopyConstructor_Test()
	{
		var expected = new Anime(@"C:\anime.amq", new AnimeBase
		{
			Id = 73,
			Name = "Anime",
			Songs =
			[
				new()
				{
					Artist = "Artist",
					Name = "Name",
					Type = SongType.Op.Create(1),
				},
			],
			Source = @"C:\video.mkv",
			Year = 1984,
		}, null);
		var actual = new Anime(expected);

		actual.Should().BeEquivalentTo(expected);
		((object)actual.Songs).Should().NotBe(expected.Songs);

		actual.Songs.Clear();
		actual.Should().NotBeEquivalentTo(expected);
	}
}