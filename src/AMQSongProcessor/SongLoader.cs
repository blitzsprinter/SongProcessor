using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor
{
	public class SongLoader : ISongLoader
	{
		private readonly ISourceInfoGatherer _Gatherer;
		private readonly JsonSerializerOptions _Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			IgnoreReadOnlyProperties = true,
		};
		public IgnoreExceptions ExceptionsToIgnore { get; set; } = IgnoreExceptions.Video;
		public string Extension { get; set; } = "amq";
		public bool RemoveIgnoredSongs { get; set; } = true;

		public SongLoader(ISourceInfoGatherer gatherer)
		{
			_Gatherer = gatherer;
			_Options.Converters.Add(new JsonStringEnumConverter());
			_Options.Converters.Add(new SongTypeAndPositionJsonConverter());
			_Options.Converters.Add(new TimeSpanJsonConverter());
			_Options.Converters.Add(new VolumeModifierConverter());
		}

		public async Task<IAnime?> LoadAsync(string file)
		{
			var fileInfo = new FileInfo(file);
			if (!fileInfo.Exists || fileInfo.Length == 0)
			{
				return null;
			}

			using var fs = new FileStream(file, FileMode.Open);

			AnimeModel model;
			try
			{
				model = await JsonSerializer.DeserializeAsync<AnimeModel>(fs, _Options).CAF();
				if (RemoveIgnoredSongs)
				{
					model.Songs.RemoveAll(x => x.ShouldIgnore);
				}
			}
			catch (JsonException) when ((ExceptionsToIgnore & IgnoreExceptions.Json) != 0)
			{
				return null;
			}
			catch (Exception e)
			{
				throw new JsonException($"Unable to parse {file}.", e);
			}

			return await ConvertFromModelAsync(file, model).CAF();
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
				var showDir = FileUtils.RemoveInvalidPathChars($"[{anime.Year}] {anime.Name}");
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

		protected virtual async Task<IAnime> ConvertFromModelAsync(string file, AnimeModel model)
		{
			var directory = Path.GetDirectoryName(file);
			var source = FileUtils.EnsureAbsolutePath(directory, model.Source);

			SourceInfo<VideoInfo>? videoInfo = null;
			if (source != null)
			{
				try
				{
					videoInfo = await _Gatherer.GetVideoInfoAsync(source).CAF();
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

		protected virtual Task<AnimeModel> ConvertToModelAsync(string file, IAnimeBase anime)
			=> Task.FromResult(new AnimeModel(anime));

		private async Task<string?> SaveAsync(string file, IAnimeBase anime)
		{
			if (string.IsNullOrWhiteSpace(file))
			{
				throw new ArgumentNullException(nameof(file));
			}

			try
			{
				using var fs = new FileStream(file, FileMode.Create);

				var model = await ConvertToModelAsync(file, anime).CAF();
				await JsonSerializer.SerializeAsync(fs, model, _Options).CAF();
				return file;
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Unable to save {anime.Name} to {file}.", e);
			}
		}
	}
}