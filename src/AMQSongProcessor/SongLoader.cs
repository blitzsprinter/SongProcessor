using System.Text.Json;
using System.Text.Json.Serialization;

using AMQSongProcessor.Converters;
using AMQSongProcessor.FFmpeg;
using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor
{
	public class SongLoader : ISongLoader
	{
		public IgnoreExceptions ExceptionsToIgnore { get; set; } = IgnoreExceptions.Video;
		public string Extension { get; set; } = "amq";
		protected ISourceInfoGatherer Gatherer { get; }
		protected Type ModelType { get; set; } = typeof(AnimeBase);
		protected JsonSerializerOptions Options { get; set; } = new()
		{
			WriteIndented = true,
			IgnoreReadOnlyProperties = true,
		};

		public SongLoader(ISourceInfoGatherer gatherer)
		{
			Gatherer = gatherer;
			Options.Converters.Add(new JsonStringEnumConverter());
			Options.Converters.Add(new SongTypeAndPositionJsonConverter());
			Options.Converters.Add(new TimeSpanJsonConverter());
			Options.Converters.Add(new VolumeModifierConverter());
			Options.Converters.Add(new AspectRatioJsonConverter());
			Options.Converters.Add(new InterfaceConverter<Song, ISong>());
		}

		public async Task<IAnime?> LoadAsync(string path)
		{
			var fileInfo = new FileInfo(path);
			if (!fileInfo.Exists || fileInfo.Length == 0)
			{
				return null;
			}

			try
			{
				object? deserialized;
				using (var fs = new FileStream(path, FileMode.Open))
				{
					deserialized = await JsonSerializer.DeserializeAsync(fs, ModelType, Options).ConfigureAwait(false);
				}

				if (deserialized is not IAnimeBase model)
				{
					throw new InvalidOperationException("Invalid type supplied for deserializing.");
				}
				return await ConvertFromModelAsync(path, model).ConfigureAwait(false);
			}
			catch (JsonException) when ((ExceptionsToIgnore & IgnoreExceptions.Json) != 0)
			{
				return null;
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
			if (options == null)
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

		protected virtual async Task<IAnime> ConvertFromModelAsync(string file, IAnimeBase model)
		{
			var directory = Path.GetDirectoryName(file);
			var source = FileUtils.EnsureAbsolutePath(directory, model.Source);

			SourceInfo<VideoInfo>? videoInfo = null;
			if (source != null)
			{
				try
				{
					videoInfo = await Gatherer.GetVideoInfoAsync(source).ConfigureAwait(false);
				}
				catch (Exception) when ((ExceptionsToIgnore & IgnoreExceptions.Video) != 0)
				{
				}
				catch (Exception e)
				{
					throw new InvalidOperationException($"Unable to get video info for {source}.", e);
				}
			}

			return new Anime(file, model, videoInfo);
		}

		protected virtual Task<IAnimeBase> ConvertToModelAsync(string file, IAnimeBase anime)
			=> Task.FromResult<IAnimeBase>(new AnimeBase(anime));

		private async Task<string?> SaveAsync(string file, IAnimeBase anime)
		{
			if (string.IsNullOrWhiteSpace(file))
			{
				throw new ArgumentNullException(nameof(file));
			}

			try
			{
				var model = await ConvertToModelAsync(file, anime).ConfigureAwait(false);
				using (var fs = new FileStream(file, FileMode.Create))
				{
					await JsonSerializer.SerializeAsync(fs, model, ModelType, Options).ConfigureAwait(false);
				}

				return file;
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Unable to save {anime.Name} to {file}.", e);
			}
		}
	}
}