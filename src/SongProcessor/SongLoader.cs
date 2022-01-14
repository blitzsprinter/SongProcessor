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

	public async Task<Anime?> LoadAsync(string path)
	{
		var file = new FileInfo(path);
		if (!file.Exists || file.Length == 0)
		{
			return null;
		}

		AnimeBase model;
		try
		{
			using var fs = file.OpenRead();
			model = (await JsonSerializer.DeserializeAsync<AnimeBase>(fs, _Options).ConfigureAwait(false))!;
		}
		catch (Exception e)
		{
			throw new JsonException($"Unable to parse {file}.", e);
		}

		SourceInfo<VideoInfo>? videoInfo = null;
		var source = FileUtils.EnsureAbsolutePath(file.DirectoryName!, model.Source);
		if (source is not null)
		{
			try
			{
				videoInfo = await _Gatherer.GetVideoInfoAsync(source).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Unable to get video info for {source}.", e);
			}
		}

		return new Anime(file.FullName, model, videoInfo);
	}

	public async Task SaveAsync(string path, IAnimeBase anime)
	{
		var file = new FileInfo(path);
		var model = new AnimeBase(anime);
		try
		{
			using var fs = file.Open(FileMode.Create);
			await JsonSerializer.SerializeAsync(fs, model, _Options).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			throw new InvalidOperationException($"Unable to save {anime.Name} to {file}.", e);
		}
	}

	async Task<IAnime?> ISongLoader.LoadAsync(string path)
		=> await LoadAsync(path).ConfigureAwait(false);
}