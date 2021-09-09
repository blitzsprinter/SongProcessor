using SongProcessor.Converters;
using SongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SongProcessor.Tests.Converters
{
	[TestClass]
	public sealed class VolumeModifierJsonConverter_Tests
		: JsonConverter_TestsBase<VolumeModifer, VolumeModifierJsonConverter>
	{
		public override VolumeModifierJsonConverter Converter { get; } = new();
		public override string Json { get; } = "{\"Value\":\"4.5dB\"}";
		public override VolumeModifer Value { get; } = VolumeModifer.FromDecibels(4.5);
	}
}