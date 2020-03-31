using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
		private static readonly HashSet<char> _InvalidChars
			= new HashSet<char>(Path.GetInvalidFileNameChars());

		public static Task SaveAsync(this ISongLoader loader, Anime anime, string dir)
		{
			var sb = new StringBuilder($"[{anime.Year}] ");
			foreach (var c in anime.Name)
			{
				if (!_InvalidChars.Contains(c))
				{
					sb.Append(c);
				}
			}

			var animeDir = Path.Combine(dir, sb.ToString());
			Directory.CreateDirectory(animeDir);
			anime.InfoFile = Path.Combine(animeDir, $"info.{loader.Extension}");
			return loader.SaveAsync(anime);
		}
	}

	public sealed class SongLoader : ISongLoader
	{
		private readonly JsonSerializerOptions _Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			IgnoreReadOnlyProperties = true,
		};

		public string Extension { get; set; } = "amq";
		public bool RemoveIgnoredSongs { get; set; } = true;

		public SongLoader()
		{
			_Options.Converters.Add(new JsonStringEnumConverter());
			_Options.Converters.Add(new SongTypeAndPositionJsonConverter());
			_Options.Converters.Add(new TimeSpanJsonConverter());
			_Options.Converters.Add(new VolumeModifierConverter());
		}

		public async IAsyncEnumerable<Anime> LoadAsync(string dir)
		{
			var gatherer = new SourceInfoGatherer();
			foreach (var file in Directory.EnumerateFiles(dir, $"*.{Extension}", SearchOption.AllDirectories))
			{
				using var fs = new FileStream(file, FileMode.Open);

				var show = await JsonSerializer.DeserializeAsync<Anime>(fs, _Options).CAF();
				show.InfoFile = file;
				show.Songs = new SongCollection(show, show.Songs);
				if (RemoveIgnoredSongs)
				{
					show.Songs.RemoveAll(x => x.ShouldIgnore);
				}
				if (show.GetSourcePath() is string source)
				{
					show.VideoInfo = await gatherer.GetVideoInfoAsync(source).CAF();
				}

				yield return show;
			}
		}

		public async Task<Anime> LoadFromANNAsync(int id)
			=> await ANNGatherer.GetAsync(id).CAF();

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