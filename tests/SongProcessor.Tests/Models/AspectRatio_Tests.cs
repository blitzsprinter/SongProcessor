using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Models;

namespace SongProcessor.Tests.Models;

[TestClass]
public sealed class AspectRatio_Tests
{
	public const char SEP = AspectRatio.SEPARATOR;
	public static AspectRatio Default { get; } = new(16, 9);

	[TestMethod]
	public void CompareTo_Test()
	{
		var range = Enumerable.Range(1, 999);
		var expected = range.Select(x => new AspectRatio(x, 1)).ToList();

		var ratios = new SortedList<AspectRatio, AspectRatio>();
		var rng = new Random(0);

		var widths = range.ToList();
		while (widths.Count > 0)
		{
			var index = rng.Next(0, widths.Count);
			var ratio = new AspectRatio(widths[index], 1);
			widths.RemoveAt(index);
			ratios.Add(ratio, ratio);
		}

		ratios.Values.Should().BeEquivalentTo(expected);
	}

	[TestMethod]
	public void ConstructorInvalidHeight_Test()
		=> ConstructorFailure_Test(x => new(1, x));

	[TestMethod]
	public void ConstructorInvalidWidth_Test()
		=> ConstructorFailure_Test(x => new(x, 1));

	[TestMethod]
	public void EqualsDifferentValuesSameRatio_Test()
	{
		var doubled = new AspectRatio(Default.Width * 2, Default.Height * 2);
		doubled.Should().NotBe(Default);
		doubled.Ratio.Should().Be(Default.Ratio);
	}

	[TestMethod]
	public void EqualsNullable_Test()
	{
		AspectRatio? nullable = new AspectRatio(16, 9);
		nullable.Should().Be(Default);

		nullable = null;
		nullable.Should().NotBe(Default);
	}

	[TestMethod]
	public void EqualsOperator_Test()
	{
		var other = new AspectRatio(16, 10);
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
		const int Y = 100;

		var set = new HashSet<AspectRatio>();
		for (var i = 0; i < 2; ++i)
		{
			for (var x = 1; x < X; ++x)
			{
				for (var y = 1; y < Y; ++y)
				{
					set.Add(new AspectRatio(x, y));
				}
			}
		}
		set.Count.Should().Be((X - 1) * (Y - 1));
	}

	[TestMethod]
	public void ParseInvalidHeight_Test()
		=> ParseFailure_Test($"1{SEP}a");

	[TestMethod]
	public void ParseInvalidWidth_Test()
		=> ParseFailure_Test($"a{SEP}1");

	[TestMethod]
	public void ParseNoSplit_Test()
		=> ParseFailure_Test("nosplit");

	[TestMethod]
	public void ParseNull_Test()
		=> ParseFailure_Test(null!);

	[TestMethod]
	public void ParseSuccess_Test()
	{
		var result = AspectRatio.Parse($"{Default.Width}{SEP}{Default.Height}", SEP);
		result.Should().Be(Default);
	}

	[TestMethod]
	public void ParseTooManyParts_Test()
		=> ParseFailure_Test($"1{SEP}2{SEP}3");

	[TestMethod]
	public void ParseToString_Test()
		=> AspectRatio.Parse(Default.ToString(), SEP).Should().Be(Default);

	private static void ConstructorFailure_Test(Func<int, AspectRatio> action)
	{
		foreach (var value in new[] { int.MinValue, -1, 0 })
		{
			value.Invoking(x => action(x))
				.Should().Throw<ArgumentOutOfRangeException>();
		}
	}

	private static void ParseFailure_Test(string input)
	{
		Action parse = () => AspectRatio.Parse(input, SEP);
		parse.Should().Throw<FormatException>();
	}
}