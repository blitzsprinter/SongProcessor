using SongProcessor.Converters;
using SongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SongProcessor.Tests.Converters
{
	[TestClass]
	public sealed class AspectRatioJsonConverter_Tests
		: JsonConverter_TestsBase<AspectRatio, AspectRatioJsonConverter>
	{
		public override AspectRatioJsonConverter Converter { get; } = new();
		public override string Json { get; } = "{\"Value\":\"16:9\"}";
		public override AspectRatio Value { get; } = new(16, 9);
	}
}