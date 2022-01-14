using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg;
using SongProcessor.Models;
using SongProcessor.Tests.FFmpeg;
using SongProcessor.Utils;

using System.Text.Json;

namespace SongProcessor.Tests;

[TestClass]
public sealed class SongLoader_Tests : FFmpeg_TestsBase
{
	private readonly ISongLoader _Loader = new SongLoader(new SourceInfoGatherer());

	[TestMethod]
	public async Task LoadFileDoesntExist_Test()
	{
		using var temp = new TempDirectory();
		var path = Path.Combine(temp.Dir, "info.amq");

		var actual = await _Loader.LoadAsync(path).ConfigureAwait(false);
		actual.Should().BeNull();
	}

	[TestMethod]
	public async Task LoadFileEmpty_Test()
	{
		using var temp = new TempDirectory();
		var path = Path.Combine(temp.Dir, "info.amq");
		File.Create(path).Dispose();

		var actual = await _Loader.LoadAsync(path).ConfigureAwait(false);
		actual.Should().BeNull();
	}

	[TestMethod]
	public async Task LoadInvalid_Test()
	{
		using var temp = new TempDirectory();
		var path = Path.Combine(temp.Dir, "info.amq");
		File.WriteAllText(path, "asdf");

		Func<Task> load = () => _Loader.LoadAsync(path);
		await load.Should().ThrowAsync<JsonException>().ConfigureAwait(false);
	}

	[TestMethod]
	public async Task LoadInvalidVideo_Test()
	{
		using var temp = new TempDirectory();
		var anime = CreateAnime(temp.Dir);

		await _Loader.SaveAsync(anime.AbsoluteInfoPath, new AnimeBase(anime)
		{
			Source = anime.AbsoluteInfoPath,
		}).ConfigureAwait(false);

		Func<Task> load = () => _Loader.LoadAsync(anime.AbsoluteInfoPath);
		await load.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false);
	}

	[TestMethod]
	public async Task SaveAndLoad_Test()
	{
		using var temp = new TempDirectory();
		var expected = CreateAnime(temp.Dir);
		expected.Songs.AddRange(new Song[]
		{
			new()
			{
				Name = "Song1",
				Artist = "Artist1",
				Type = SongType.Op.Create(1),
			},
			new()
			{
				Name = "Song2",
				Artist = "Artist2",
				Type = SongType.Ed.Create(1),
			},
		});

		await _Loader.SaveAsync(expected).ConfigureAwait(false);
		var actual = await _Loader.LoadAsync(expected.AbsoluteInfoPath).ConfigureAwait(false);
		actual.Should().BeEquivalentTo(expected,
			x => x.ComparingByMembers<SourceInfo<VideoInfo>>());
	}

	[TestMethod]
	public async Task SaveFailure_Test()
	{
		using var temp = new TempDirectory();
		var anime = CreateAnime(temp.Dir);
		using var fs = File.Create(anime.AbsoluteInfoPath);

		Func<Task> save = () => _Loader.SaveAsync(anime);
		await save.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false);
	}
}