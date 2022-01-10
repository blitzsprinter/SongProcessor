using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Gatherers;
using SongProcessor.Models;

namespace SongProcessor.Tests.Gatherers;

[TestClass]
public sealed class GatherOptions_Tests
{
	[TestMethod]
	public void AddEndings_Test()
	{
		var options = new GatherOptions(
			AddEndings: true,
			AddInserts: false,
			AddOpenings: false,
			AddSongs: true
		);
		options.CanBeGathered(SongType.Ed).Should().BeTrue();
		options.CanBeGathered(SongType.In).Should().BeFalse();
		options.CanBeGathered(SongType.Op).Should().BeFalse();
	}

	[TestMethod]
	public void AddInserts_Test()
	{
		var options = new GatherOptions(
			AddEndings: false,
			AddInserts: true,
			AddOpenings: false,
			AddSongs: true
		);
		options.CanBeGathered(SongType.Ed).Should().BeFalse();
		options.CanBeGathered(SongType.In).Should().BeTrue();
		options.CanBeGathered(SongType.Op).Should().BeFalse();
	}

	[TestMethod]
	public void AddOpenings_Test()
	{
		var options = new GatherOptions(
			AddEndings: false,
			AddInserts: false,
			AddOpenings: true,
			AddSongs: true
		);
		options.CanBeGathered(SongType.Ed).Should().BeFalse();
		options.CanBeGathered(SongType.In).Should().BeFalse();
		options.CanBeGathered(SongType.Op).Should().BeTrue();
	}

	[TestMethod]
	public void AddSongsFalse_Test()
	{
		var options = new GatherOptions(
			AddEndings: true,
			AddInserts: true,
			AddOpenings: true,
			AddSongs: false
		);
		options.CanBeGathered(SongType.Ed).Should().BeFalse();
		options.CanBeGathered(SongType.In).Should().BeFalse();
		options.CanBeGathered(SongType.Op).Should().BeFalse();
	}

	[TestMethod]
	public void InvalidSongType_Test()
	{
		var options = new GatherOptions(
			AddEndings: true,
			AddInserts: true,
			AddOpenings: true,
			AddSongs: true
		);
		Action canGather = () => _ = options.CanBeGathered((SongType)3);
		canGather.Should().Throw<ArgumentOutOfRangeException>();
	}
}