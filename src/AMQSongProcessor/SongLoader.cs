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
	public sealed class SongLoader
	{
		private readonly JsonSerializerOptions _Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			IgnoreReadOnlyProperties = true,
		};

		public bool RemoveIgnoredSongs { get; set; } = true;

		public SongLoader()
		{
			_Options.Converters.Add(new JsonStringEnumConverter());
			_Options.Converters.Add(new SongTypeAndPositionJsonConverter());
			_Options.Converters.Add(new TimeSpanJsonConverter());
		}

		public async IAsyncEnumerable<Anime> LoadAsync(string dir)
		{
			var gatherer = new SourceInfoGatherer();
			foreach (var file in Directory.EnumerateFiles(dir, "*.amq", SearchOption.AllDirectories))
			{
				using var fs = new FileStream(file, FileMode.Open);

				var show = await JsonSerializer.DeserializeAsync<Anime>(fs, _Options).CAF();
				show.Directory = Path.GetDirectoryName(file);
				if (RemoveIgnoredSongs)
				{
					show.Songs.RemoveAll(x => x.ShouldIgnore);
				}
				show.VideoInfo = await gatherer.GetVideoInfoAsync(show.GetSourcePath()).CAF();

				yield return show;
			}
		}
	}
}