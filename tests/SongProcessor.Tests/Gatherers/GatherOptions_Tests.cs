using SongProcessor.Gatherers;
using SongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SongProcessor.Tests.Gatherers
{
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
			Assert.IsTrue(options.CanBeGathered(SongType.Ed));
			Assert.IsFalse(options.CanBeGathered(SongType.In));
			Assert.IsFalse(options.CanBeGathered(SongType.Op));
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
			Assert.IsFalse(options.CanBeGathered(SongType.Ed));
			Assert.IsTrue(options.CanBeGathered(SongType.In));
			Assert.IsFalse(options.CanBeGathered(SongType.Op));
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
			Assert.IsFalse(options.CanBeGathered(SongType.Ed));
			Assert.IsFalse(options.CanBeGathered(SongType.In));
			Assert.IsTrue(options.CanBeGathered(SongType.Op));
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
			Assert.IsFalse(options.CanBeGathered(SongType.Ed));
			Assert.IsFalse(options.CanBeGathered(SongType.In));
			Assert.IsFalse(options.CanBeGathered(SongType.Op));
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
			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			{
				_ = options.CanBeGathered((SongType)3);
			});
		}
	}
}