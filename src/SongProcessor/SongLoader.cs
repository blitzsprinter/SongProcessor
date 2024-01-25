using SongProcessor.Converters;
using SongProcessor.FFmpeg;
using SongProcessor.Models;
using SongProcessor.Utils;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SongProcessor;

public sealed class SongLoader : ISongLoader
{
	private readonly ISourceInfoGatherer _Gatherer;
	private readonly JsonSerializerOptions _Options = new()
	{
		WriteIndented = true,
		IgnoreReadOnlyProperties = true,
	};
	public string Extension { get; set; } = "amq";

	public SongLoader(ISourceInfoGatherer gatherer)
	{
		_Gatherer = gatherer;
		_Options.Converters.Add(new JsonStringEnumConverter());
		_Options.Converters.Add(new SongTypeAndPositionJsonConverter());
		_Options.Converters.Add(new VolumeModifierJsonConverter());
		_Options.Converters.Add(new AspectRatioJsonConverter());
		_Options.Converters.Add(new ParseJsonConverter<TimeSpan>(TimeSpan.Parse));
		_Options.Converters.Add(new InterfaceJsonConverter<Song, ISong>());
	}

	public async Task<Anime?> LoadAsync(string file)
	{
		if (!File.Exists(file))
		{
			return null;
		}

		AnimeBase model;
		await using (var fs = File.OpenRead(file))
		{
			model = (await JsonSerializer.DeserializeAsync<AnimeBase>(fs, _Options).ConfigureAwait(false))!;
		}

		var videoInfo = default(VideoInfo?);
		var directory = Path.GetDirectoryName(file);
		var source = FileUtils.EnsureAbsoluteFile(directory!, model.Source);
		if (source is not null)
		{
			videoInfo = await _Gatherer.GetVideoInfoAsync(source).ConfigureAwait(false);
		}

		return new Anime(file, model, videoInfo);
	}

	public async Task SaveAsync(string file, IAnimeBase anime)
	{
		var model = new AnimeBase(anime);
		await using var fs = File.OpenWrite(file);
		await JsonSerializer.SerializeAsync(fs, model, _Options).ConfigureAwait(false);
	}

	async Task<IAnime?> ISongLoader.LoadAsync(string file)
		=> await LoadAsync(file).ConfigureAwait(false);
}