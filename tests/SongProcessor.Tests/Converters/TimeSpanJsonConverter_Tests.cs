using SongProcessor.Converters;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SongProcessor.Tests.Converters
{
	[TestClass]
	public sealed class TimeSpanJsonConverter_Tests
		: JsonConverter_TestsBase<TimeSpan, ParseJsonConverter<TimeSpan>>
	{
		public override ParseJsonConverter<TimeSpan> Converter { get; } = new(TimeSpan.Parse);
		public override string Json { get; } = "{\"Value\":\"02:52:07.9870000\"}";
		public override TimeSpan Value { get; } = new TimeSpan(0, 2, 52, 7, 987);
	}
}