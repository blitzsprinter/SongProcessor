using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.Converters
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