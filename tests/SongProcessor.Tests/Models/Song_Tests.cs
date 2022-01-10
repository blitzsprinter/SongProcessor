using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Models;

namespace SongProcessor.Tests.Models;

[TestClass]
public sealed class Song_Tests
{
	[TestMethod]
	public void CopyConstructor_Test()
	{
		var expected = new Song()
		{
			AlsoIn = new() { 1, 2, 3 },
			Artist = "Artist",
			CleanPath = @"C:\song.flac",
			End = TimeSpan.FromMinutes(3),
			Episode = 73,
			Name = "Name",
			OverrideAspectRatio = new(16, 9),
			OverrideAudioTrack = 10,
			OverrideVideoTrack = 10,
			ShouldIgnore = true,
			Start = TimeSpan.FromMinutes(1),
			Status = Status.Submitted,
			Type = SongType.Op.Create(1),
			VolumeModifier = VolumeModifer.FromDecibels(-2),
		};
		var actual = new Song(expected);

		actual.Should().BeEquivalentTo(expected);
		((object)actual.AlsoIn).Should().NotBe(expected.AlsoIn);

		actual.AlsoIn.Clear();
		actual.Should().NotBeEquivalentTo(expected);
	}
}