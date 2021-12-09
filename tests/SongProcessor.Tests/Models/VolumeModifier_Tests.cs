using SongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SongProcessor.Tests.Models;

[TestClass]
public sealed class VolumeModifier_Tests
{
	[TestMethod]
	public void ConstructorInvalidPercentage_Test()
	{
		foreach (var value in new[] { double.MinValue, -1, -0.01 })
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			{
				_ = VolumeModifer.FromPercentage(value);
			});
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
		=> Assert.ThrowsException<FormatException>(() => VolumeModifer.Parse(input));

	private static void ParseSuccess_Test(VolumeModifer expected, string expectedString)
	{
		var @string = expected.ToString();
		Assert.AreEqual(expectedString, @string);
		var result = VolumeModifer.Parse(@string);
		Assert.AreEqual(expected, result);
	}
}
