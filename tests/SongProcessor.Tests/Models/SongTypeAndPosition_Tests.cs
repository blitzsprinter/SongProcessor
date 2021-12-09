using SongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SongProcessor.Tests.Models;

[TestClass]
public sealed class SongTypeAndPosition_Tests
{
	public static SongTypeAndPosition Default { get; } = SongType.Op.Create(1);

	[TestMethod]
	public void CompareTo_Test()
	{
		var range = Enumerable.Range(1, 999).Cast<int?>().Prepend(null);
		var expected = new[] { 0, 1 }.Cast<SongType>().SelectMany(type =>
		{
			return range.Select(x => new SongTypeAndPosition(type, x));
		}).Append(SongType.In.Create(null)).ToList();

		var songs = new SortedList<SongTypeAndPosition, SongTypeAndPosition>();
		var rng = new Random(0);

		{
			var song = SongType.In.Create(null);
			songs.Add(song, song);
		}

		foreach (var type in new[] { 0, 1 }.Cast<SongType>())
		{
			var positions = range.ToList();
			while (positions.Count > 0)
			{
				var index = rng.Next(0, positions.Count);
				var song = new SongTypeAndPosition(type, positions[index]);
				positions.RemoveAt(index);
				songs.Add(song, song);
			}
		}

		Assert.AreEqual(expected.Count, songs.Count);
		for (var i = 0; i < expected.Count; ++i)
		{
			Assert.AreEqual(expected[i], songs.Values[i]);
		}
	}

	[TestMethod]
	public void ConstructorInvalidPosition_Test()
	{
		foreach (var value in new[] { int.MinValue, -1, 0 })
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			{
				_ = new SongTypeAndPosition(SongType.Op, value);
			});
		}
	}

	[TestMethod]
	public void ConstructorInvalidType_Test()
	{
		foreach (var value in new[] { int.MinValue, -1, 3, int.MaxValue }.Cast<SongType>())
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			{
				_ = new SongTypeAndPosition(value, null);
			});
		}
	}

	[TestMethod]
	public void EqualsNullable_Test()
	{
		SongTypeAndPosition? nullable = SongType.Op.Create(1);
		Assert.AreEqual(Default, nullable);

		nullable = null;
		Assert.AreNotEqual(Default, nullable);
	}

	[TestMethod]
	public void EqualsOperator_Test()
	{
		var other = SongType.Ed.Create(1);
		Assert.IsFalse(Default == other);
		Assert.IsTrue(Default != other);
	}

	[TestMethod]
	public void EqualsWrongType_Test()
		=> Assert.IsFalse(Default.Equals("string"));

	[TestMethod]
	public void GetHashCode_Test()
	{
		const int X = 100;
		var types = new[] { SongType.Ed, SongType.Op, SongType.In };

		var set = new HashSet<SongTypeAndPosition>();
		for (var i = 0; i < 2; ++i)
		{
			foreach (var type in types)
			{
				set.Add(new SongTypeAndPosition(type, null));
				for (var x = 1; x < X; ++x)
				{
					set.Add(new SongTypeAndPosition(type, x));
				}
			}
		}
		Assert.AreEqual((X * (types.Length - 1)) + 1, set.Count);
	}

	[TestMethod]
	public void ParseInvalidPosition_Test()
		=> ParseFailure_Test("In1a");

	[TestMethod]
	public void ParseInvalidType_Test()
		=> ParseFailure_Test("invalid_type1");

	[TestMethod]
	public void ParseNull_Test()
		=> ParseFailure_Test(null!);

	[TestMethod]
	public void ParseSuccessNoPosition_Test()
		=> ParseSuccess_Test(SongType.Ed.Create(null), "Ending");

	[TestMethod]
	public void ParseSuccessWithPosition_Test()
		=> ParseSuccess_Test(SongType.Ed.Create(1), "Ending 1");

	[TestMethod]
	public void ToStringEnding_Test()
	{
		Assert.AreEqual("Ending", SongType.Ed.Create(null).ToString());
		Assert.AreEqual("Ed", SongType.Ed.Create(null).ToString(shortType: true));
		Assert.AreEqual("Ending 1", SongType.Ed.Create(1).ToString());
		Assert.AreEqual("Ed 1", SongType.Ed.Create(1).ToString(shortType: true));
	}

	[TestMethod]
	public void ToStringInsert_Test()
	{
		Assert.AreEqual("Insert", SongType.In.Create(null).ToString());
		Assert.AreEqual("In", SongType.In.Create(null).ToString(shortType: true));
		Assert.AreEqual("Insert", SongType.In.Create(1).ToString());
		Assert.AreEqual("In", SongType.In.Create(1).ToString(shortType: true));
	}

	[TestMethod]
	public void ToStringOpening_Test()
	{
		Assert.AreEqual("Opening", SongType.Op.Create(null).ToString());
		Assert.AreEqual("Op", SongType.Op.Create(null).ToString(shortType: true));
		Assert.AreEqual("Opening 1", SongType.Op.Create(1).ToString());
		Assert.AreEqual("Op 1", SongType.Op.Create(1).ToString(shortType: true));
	}

	private static void ParseFailure_Test(string input)
		=> Assert.ThrowsException<FormatException>(() => SongTypeAndPosition.Parse(input));

	private static void ParseSuccess_Test(SongTypeAndPosition expected, string expectedString)
	{
		var @string = expected.ToString();
		Assert.AreEqual(expectedString, @string);
		var result = SongTypeAndPosition.Parse(@string);
		Assert.AreEqual(expected, result);
	}
}
