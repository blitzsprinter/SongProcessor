using System.Text.Json;
using System.Text.Json.Serialization;

using SongProcessor.Converters;
using SongProcessor.FFmpeg;
using SongProcessor.Models;
using SongProcessor.Utils;

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
		var fileInfo = new FileInfo(path);
		if (!fileInfo.Exists || fileInfo.Length == 0)
		{
			return null;
		}

		try
		{
			AnimeBase model;
			using (var fs = new FileStream(path, FileMode.Open))
			{
				model = (await JsonSerializer.DeserializeAsync<AnimeBase>(fs, _Options).ConfigureAwait(false))!;
			}
			return await ConvertFromModelAsync(path, model).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			throw new JsonException($"Unable to parse {path}.", e);
		}
	}

	public Task<string?> SaveAsync(string path, IAnimeBase anime, SaveNewOptions? options = null)
	{
		if (Path.IsPathFullyQualified(path) && !string.IsNullOrEmpty(Path.GetExtension(path)))
		{
			return SaveAsync(path, anime);
		}
		if (options is null)
		{
			return SaveAsync(Path.Combine(path, $"info.{Extension}"), anime);
		}

		var fullDir = path;
		if (options.AddShowNameDirectory)
		{
			var showDir = FileUtils.SanitizePath($"[{anime.Year}] {anime.Name}");
			fullDir = Path.Combine(path, showDir);
		}
		Directory.CreateDirectory(fullDir);

		var file = Path.Combine(fullDir, $"info.{Extension}");
		var fileExists = File.Exists(file);
		if (fileExists && options.CreateDuplicateFile)
		{
			file = FileUtils.NextAvailableFilename(file);
			fileExists = false;
		}

		if (fileExists && !options.AllowOverwrite)
		{
			return Task.FromResult(default(string?));
		}
		return SaveAsync(file, anime);
	}

	async Task<IAnime?> ISongLoader.LoadAsync(string path)
		=> await LoadAsync(path).ConfigureAwait(false);

	private async Task<Anime> ConvertFromModelAsync(string path, AnimeBase model)
	{
		var directory = Path.GetDirectoryName(path)!;
		var source = FileUtils.EnsureAbsolutePath(directory, model.Source);

		SourceInfo<VideoInfo>? videoInfo = null;
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

		return new Anime(path, model, videoInfo);
	}

	private async Task<string?> SaveAsync(string path, IAnimeBase anime)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			throw new ArgumentNullException(nameof(path));
		}

		try
		{
			var model = new AnimeBase(anime);
			using (var fs = new FileStream(path, FileMode.Create))
			{
				await JsonSerializer.SerializeAsync(fs, model, _Options).ConfigureAwait(false);
			}
			return path;
		}
		catch (Exception e)
		{
			throw new InvalidOperationException($"Unable to save {anime.Name} to {path}.", e);
		}
	}
}
