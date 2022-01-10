using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Models;

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

		songs.Values.Should().BeEquivalentTo(expected);
	}

	[TestMethod]
	public void ConstructorInvalidPosition_Test()
	{
		foreach (var value in new[] { int.MinValue, -1, 0 })
		{
			value.Invoking(x => new SongTypeAndPosition(SongType.Op, x))
				.Should().Throw<ArgumentOutOfRangeException>();
		}
	}

	[TestMethod]
	public void ConstructorInvalidType_Test()
	{
		foreach (var value in new[] { int.MinValue, -1, 3, int.MaxValue }.Cast<SongType>())
		{
			value.Invoking(x => new SongTypeAndPosition(x, null))
				.Should().Throw<ArgumentOutOfRangeException>();
		}
	}

	[TestMethod]
	public void EqualsNullable_Test()
	{
		SongTypeAndPosition? nullable = SongType.Op.Create(1);
		nullable.Should().Be(Default);

		nullable = null;
		nullable.Should().NotBe(Default);
	}

	[TestMethod]
	public void EqualsOperator_Test()
	{
		var other = SongType.Ed.Create(1);
		(Default == other).Should().BeFalse();
		(Default != other).Should().BeTrue();
	}

	[TestMethod]
	public void EqualsWrongType_Test()
		=> Default.Equals("string").Should().BeFalse();

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
		set.Count.Should().Be((X * (types.Length - 1)) + 1);
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
		SongType.Ed.Create(null).ToString().Should().Be("Ending");
		SongType.Ed.Create(null).ToString(shortType: true).Should().Be("Ed");
		SongType.Ed.Create(1).ToString().Should().Be("Ending 1");
		SongType.Ed.Create(1).ToString(shortType: true).Should().Be("Ed 1");
	}

	[TestMethod]
	public void ToStringInsert_Test()
	{
		SongType.In.Create(null).ToString().Should().Be("Insert");
		SongType.In.Create(null).ToString(shortType: true).Should().Be("In");
		SongType.In.Create(1).ToString().Should().Be("Insert");
		SongType.In.Create(1).ToString(shortType: true).Should().Be("In");
	}

	[TestMethod]
	public void ToStringOpening_Test()
	{
		SongType.Op.Create(null).ToString().Should().Be("Opening");
		SongType.Op.Create(null).ToString(shortType: true).Should().Be("Op");
		SongType.Op.Create(1).ToString().Should().Be("Opening 1");
		SongType.Op.Create(1).ToString(shortType: true).Should().Be("Op 1");
	}

	private static void ParseFailure_Test(string input)
	{
		Action parse = () => SongTypeAndPosition.Parse(input);
		parse.Should().Throw<FormatException>();
	}

	private static void ParseSuccess_Test(SongTypeAndPosition expected, string expectedString)
	{
		var @string = expected.ToString();
		@string.Should().Be(expectedString);
		var result = SongTypeAndPosition.Parse(@string);
		result.Should().Be(expected);
	}
}