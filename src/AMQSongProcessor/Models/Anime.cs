using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
	public class Anime : IAnime
	{
		public string AbsoluteInfoPath { get; }
		public int Id { get; }
		public string Name { get; }
		public List<Song> Songs { get; } = new();
		public string? Source => FileUtils.StoreRelativeOrAbsolute(this.GetDirectory(), VideoInfo?.Path);
		public SourceInfo<VideoInfo>? VideoInfo { get; set; }
		public int Year { get; }
		IReadOnlyList<ISong> IAnimeBase.Songs => Songs;
		private string DebuggerDisplay => Name;

		public Anime(string file, IAnimeBase other, SourceInfo<VideoInfo>? videoInfo)
		{
			if (file is null)
			{
				throw new ArgumentNullException(nameof(file));
			}
			if (Path.GetDirectoryName(file) is null)
			{
				throw new ArgumentException("Must be an absolute path.", nameof(file));
			}

			AbsoluteInfoPath = file;
			Id = other.Id;
			Name = other.Name;
			Songs = other.Songs?.Select(x => new Song(x))?.ToList() ?? new List<Song>();
			Year = other.Year;
			VideoInfo = videoInfo;
		}

		public Anime(IAnime anime) : this(anime.AbsoluteInfoPath, anime, anime.VideoInfo)
		{
		}
	}
}