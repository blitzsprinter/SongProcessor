using SongProcessor.Converters;
using SongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SongProcessor.Tests.Converters
{
	[TestClass]
	public sealed class SongTypeAndPositionJsonConverter_Tests
		: JsonConverter_TestsBase<SongTypeAndPosition, SongTypeAndPositionJsonConverter>
	{
		public override SongTypeAndPositionJsonConverter Converter { get; } = new();
		public override string Json { get; } = "{\"Value\":\"Ed 3\"}";
		public override SongTypeAndPosition Value { get; } = SongType.Ed.Create(3);
	}
}