using System;
using System.Collections.Generic;
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
	public sealed class SongLoader : ISongLoader
	{
		private readonly ISourceInfoGatherer _Gatherer;
		private readonly JsonSerializerOptions _Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			IgnoreReadOnlyProperties = true,
		};
		public bool DontThrowVideoExceptions { get; set; } = true;
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

		public async Task<Anime> LoadAsync(string file)
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

			show.AbsoluteInfoPath = file;
			show.Songs = new SongCollection(show, show.Songs);
			if (RemoveIgnoredSongs)
			{
				show.Songs.RemoveAll(x => x.ShouldIgnore);
			}
			if (show.AbsoluteSourcePath is string source)
			{
				try
				{
					show.VideoInfo = await _Gatherer.GetVideoInfoAsync(source).CAF();
				}
				catch (Exception) when (DontThrowVideoExceptions)
				{
				}
			}
			return show;
		}

		public Task SaveAsync(Anime anime, SaveNewOptions? options = null)
		{
			if (options == null)
			{
				return SaveAsync(anime);
			}

			var fullDir = options.Directory;
			if (options.AddShowNameDirectory)
			{
				var showDir = FileUtils.RemoveInvalidPathChars($"[{anime.Year}] {anime.Name}");
				fullDir = Path.Combine(options.Directory, showDir);
			}
			Directory.CreateDirectory(fullDir);

			var file = Path.Combine(fullDir, $"info.{Extension}");
			var fileExists = File.Exists(file);
			if (fileExists && options.CreateDuplicateFile)
			{
				file = FileUtils.NextAvailableFilename(file);
				fileExists = false;
			}
			anime.AbsoluteInfoPath = file;

			if (fileExists && !options.AllowOverwrite)
			{
				return Task.CompletedTask;
			}
			return SaveAsync(anime);
		}

		private async Task SaveAsync(Anime anime)
		{
			if (string.IsNullOrWhiteSpace(anime.AbsoluteInfoPath))
			{
				throw new ArgumentNullException(nameof(anime.AbsoluteInfoPath));
			}

			try
			{
				using var fs = new FileStream(anime.AbsoluteInfoPath, FileMode.Create);

				await JsonSerializer.SerializeAsync(fs, anime, _Options).CAF();
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Unable to save {anime.Name} to {anime.AbsoluteInfoPath}.", e);
			}
		}
	}
}