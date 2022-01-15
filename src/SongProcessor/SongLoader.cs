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
		var fileInfo = new FileInfo(file);
		if (!fileInfo.Exists || fileInfo.Length == 0)
		{
			return null;
		}

		AnimeBase model;
		try
		{
			using var fs = fileInfo.OpenRead();
			model = (await JsonSerializer.DeserializeAsync<AnimeBase>(fs, _Options).ConfigureAwait(false))!;
		}
		catch (Exception e)
		{
			throw new JsonException($"Unable to parse {fileInfo}.", e);
		}

		var videoInfo = default(VideoInfo?);
		var source = FileUtils.EnsureAbsoluteFile(fileInfo.DirectoryName!, model.Source);
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

		return new Anime(fileInfo.FullName, model, videoInfo);
	}

	public async Task SaveAsync(string file, IAnimeBase anime)
	{
		var fileInfo = new FileInfo(file);
		var model = new AnimeBase(anime);
		try
		{
			using var fs = fileInfo.Open(FileMode.Create);
			await JsonSerializer.SerializeAsync(fs, model, _Options).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			throw new InvalidOperationException($"Unable to save {anime.Name} to {fileInfo}.", e);
		}
	}

	async Task<IAnime?> ISongLoader.LoadAsync(string file)
		=> await LoadAsync(file).ConfigureAwait(false);
}