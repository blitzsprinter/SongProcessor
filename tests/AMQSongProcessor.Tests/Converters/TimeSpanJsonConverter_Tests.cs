using AMQSongProcessor.Converters;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.Converters
{
	[TestClass]
	public sealed class TimeSpanJsonConverter_Tests
		: JsonConverter_TestsBase<TimeSpan, TimeSpanJsonConverter>
	{
		public override TimeSpanJsonConverter Converter { get; } = new();
		public override string Json { get; } = "{\"Value\":\"02:52:07.9870000\"}";
		public override TimeSpan Value { get; } = new TimeSpan(0, 2, 52, 7, 987);
	}
}