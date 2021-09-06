using AMQSongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.Models
{
	[TestClass]
	public sealed class AspectRatio_Tests
	{
		private const char SEP = AspectRatio.SEPARATOR;
		private static readonly AspectRatio Default = new(16, 9);

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

			Assert.AreEqual(expected.Count, ratios.Count);
			for (var i = 0; i < expected.Count; ++i)
			{
				Assert.AreEqual(expected[i], ratios.Values[i]);
			}
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
			Assert.AreNotEqual(Default, doubled);
			Assert.AreEqual(Default.Ratio, doubled.Ratio);
		}

		[TestMethod]
		public void EqualsNullable_Test()
		{
			AspectRatio? nullable = new AspectRatio(16, 9);
			Assert.AreEqual(Default, nullable);

			nullable = null;
			Assert.AreNotEqual(Default, nullable);
		}

		[TestMethod]
		public void EqualsOperator_Test()
		{
			var other = new AspectRatio(16, 10);
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
			Assert.AreEqual((X - 1) * (Y - 1), set.Count);
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
			Assert.AreEqual(Default.Width, result.Width);
			Assert.AreEqual(Default.Height, result.Height);
			Assert.AreEqual(Default.Width / (float)Default.Height, result.Ratio);
		}

		[TestMethod]
		public void ParseTooManyParts_Test()
			=> ParseFailure_Test($"1{SEP}2{SEP}3");

		[TestMethod]
		public void ParseToString_Test()
			=> Assert.AreEqual(Default, AspectRatio.Parse(Default.ToString(), SEP));

		private static void ConstructorFailure_Test(Func<int, AspectRatio> action)
		{
			foreach (var value in new[] { int.MinValue, -1, 0 })
			{
				Assert.ThrowsException<ArgumentOutOfRangeException>(() => action(value));
			}
		}

		private static void ParseFailure_Test(string input)
			=> Assert.ThrowsException<FormatException>(() => AspectRatio.Parse(input, SEP));
	}
}