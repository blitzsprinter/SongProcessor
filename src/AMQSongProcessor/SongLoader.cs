using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public static class SongLoaderUtils
	{
		public static Task SaveNewAsync(this ISongLoader loader, Anime anime, SaveNewOptions options)
		{
			var fullDir = options.Directory;
			if (options.AddShowNameDirectory)
			{
				var showDir = Utils.RemoveInvalidPathChars($"[{anime.Year}] {anime.Name}");
				fullDir = Path.Combine(options.Directory, showDir);
			}
			Directory.CreateDirectory(fullDir);

			var file = Path.Combine(fullDir, $"info.{loader.Extension}");
			var fileExists = File.Exists(file);
			if (fileExists && options.CreateDuplicateFile)
			{
				file = Utils.NextAvailableFilename(file);
				fileExists = false;
			}
			anime.InfoFile = file;

			if (fileExists && !options.AllowOverwrite)
			{
				return Task.CompletedTask;
			}
			return loader.SaveAsync(anime);
		}
	}

	public sealed class SongLoader : ISongLoader
	{
		private readonly ISourceInfoGatherer _Gatherer;
		private readonly JsonSerializerOptions _Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			IgnoreReadOnlyProperties = true,
		};
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

		public async Task<Song> DuplicateSongAsync(Song song)
		{
			using var ms = new MemoryStream();

			await JsonSerializer.SerializeAsync(ms, song, _Options).CAF();
			ms.Position = 0;
			return await JsonSerializer.DeserializeAsync<Song>(ms, _Options).CAF();
		}

		public async IAsyncEnumerable<Anime> LoadAsync(string dir)
		{
			foreach (var file in Directory.EnumerateFiles(dir, $"*.{Extension}", SearchOption.AllDirectories))
			{
				using var fs = new FileStream(file, FileMode.Open);

				Anime show;
				try
				{
					show = await JsonSerializer.DeserializeAsync<Anime>(fs, _Options).CAF();
				}
				catch (Exception e)
				{
					throw new JsonException($"Unable to parse {file}", e);
				}

				show.InfoFile = file;
				show.Songs = new SongCollection(show, show.Songs);
				if (RemoveIgnoredSongs)
				{
					show.Songs.RemoveAll(x => x.ShouldIgnore);
				}
				if (show.GetSourcePath() is string source)
				{
					show.VideoInfo = await _Gatherer.GetVideoInfoAsync(source).CAF();
				}

				yield return show;
			}
		}

		public Task<Anime> LoadFromANNAsync(int id)
			=> ANNGatherer.GetAsync(id);

		public async Task SaveAsync(Anime anime)
		{
			if (string.IsNullOrWhiteSpace(anime.InfoFile))
			{
				throw new ArgumentNullException(nameof(anime.InfoFile));
			}

			try
			{
				using var fs = new FileStream(anime.InfoFile, FileMode.Create);

				await JsonSerializer.SerializeAsync(fs, anime, _Options).CAF();
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Unable to save {anime.Name} to {anime.InfoFile}.", e);
			}
		}
	}
}