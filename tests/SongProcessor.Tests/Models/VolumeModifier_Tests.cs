using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Models;

namespace SongProcessor.Tests.Models;

[TestClass]
public sealed class VolumeModifier_Tests
{
	[TestMethod]
	public void ConstructorInvalidPercentage_Test()
	{
		foreach (var value in new[] { double.MinValue, -1, -0.01 })
		{
			value.Invoking(x => _ = VolumeModifer.FromPercentage(x))
				.Should().Throw<ArgumentOutOfRangeException>();
		}
	}

	[TestMethod]
	public void ParseInvalidDecibels_Test()
		=> ParseFailure_Test($"1.2asdf{VolumeModifer.DB}");

	[TestMethod]
	public void ParseInvalidPercentage_Test()
		=> ParseFailure_Test("1.2asdf");

	[TestMethod]
	public void ParseNull_Test()
		=> ParseFailure_Test(null!);

	[TestMethod]
	public void ParseSuccessDecibels_Test()
		=> ParseSuccess_Test(VolumeModifer.FromDecibels(1.2), "1.2dB");

	[TestMethod]
	public void ParseSuccessPercentage_Test()
		=> ParseSuccess_Test(VolumeModifer.FromPercentage(1.2), "1.2");

	private static void ParseFailure_Test(string input)
	{
		Action parse = () => VolumeModifer.Parse(input);
		parse.Should().Throw<FormatException>();
	}

	private static void ParseSuccess_Test(VolumeModifer expected, string expectedString)
	{
		var @string = expected.ToString();
		@string.Should().Be(expectedString);
		var result = VolumeModifer.Parse(@string);
		result.Should().Be(expected);
	}
}