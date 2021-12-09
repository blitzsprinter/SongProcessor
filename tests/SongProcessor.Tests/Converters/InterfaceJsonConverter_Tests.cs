using System.Text.Json;

using SongProcessor.Converters;
using SongProcessor.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
	{
		Assert.AreEqual(Value.AlsoIn.Count, actual.AlsoIn.Count);
		Assert.AreEqual(Value.Artist, actual.Artist);
		Assert.AreEqual(Value.CleanPath, actual.CleanPath);
		Assert.AreEqual(Value.End, actual.End);
		Assert.AreEqual(Value.Episode, actual.Episode);
		Assert.AreEqual(Value.Name, actual.Name);
		Assert.AreEqual(Value.OverrideAspectRatio, actual.OverrideAspectRatio);
		Assert.AreEqual(Value.OverrideAudioTrack, actual.OverrideAudioTrack);
		Assert.AreEqual(Value.OverrideVideoTrack, actual.OverrideVideoTrack);
		Assert.AreEqual(Value.ShouldIgnore, actual.ShouldIgnore);
		Assert.AreEqual(Value.Start, actual.Start);
		Assert.AreEqual(Value.Status, actual.Status);
		Assert.AreEqual(Value.Type, actual.Type);
		Assert.AreEqual(Value.VolumeModifier, actual.VolumeModifier);
	}

	protected override void ConfigureOptions(JsonSerializerOptions options)
	{
		options.Converters.Add(new SongTypeAndPositionJsonConverter());
	}
}
