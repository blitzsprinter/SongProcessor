using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Anime : IAnime
	{
		public string AbsoluteInfoPath { get; }
		public int Id { get; }
		public string Name { get; }
		public IList<Song> Songs { get; } = new List<Song>();
		public string? Source => FileUtils.StoreRelativeOrAbsolute(this.GetDirectory(), VideoInfo?.Path);
		public SourceInfo<VideoInfo>? VideoInfo { get; set; }
		public int Year { get; }
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
			Songs = other.Songs?.Select(x => x.DeepCopy())?.ToList() ?? new List<Song>();
			Year = other.Year;
			VideoInfo = videoInfo;
		}
	}
}