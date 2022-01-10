using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Converters;
using SongProcessor.Models;

using System.Text.Json;

namespace SongProcessor.Tests.Converters;

[TestClass]
public sealed class InterfaceJsonConverter_Tests
	: JsonConverter_TestsBase<ISong, InterfaceJsonConverter<Song, ISong>>
{
	public override InterfaceJsonConverter<Song, ISong> Converter { get; } = new();
	public override string Json { get; } = "{\"Value\":{" +
		"\"AlsoIn\":[]," +
		"\"Artist\":\"Miho Fujiwara\"," +
		"\"CleanPath\":null," +
		"\"End\":\"00:00:00\"," +
		"\"Episode\":null," +
		"\"Name\":\"Streets Are Hot\"," +
		"\"OverrideAspectRatio\":null," +
		"\"OverrideAudioTrack\":0," +
		"\"OverrideVideoTrack\":0," +
		"\"ShouldIgnore\":false," +
		"\"Start\":\"00:00:00\"," +
		"\"Status\":\"Submitted, Mp3, Res480\"," +
		"\"Type\":\"Ed\"," +
		"\"VolumeModifier\":null" +
		"}}";
	public override ISong Value { get; } = new Song
	{
		Artist = "Miho Fujiwara",
		Name = "Streets Are Hot",
		Status = Status.Submitted | Status.Mp3 | Status.Res480,
		Type = SongType.Ed.Create(null),
	};

	protected override void AssertEqual(ISong actual)
		=> actual.Should().BeEquivalentTo(Value);

	protected override JsonSerializerOptions CreateOptions()
	{
		var options = base.CreateOptions();
		options.Converters.Add(new SongTypeAndPositionJsonConverter());
		return options;
	}
}