using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;

using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Anime
	{
		[JsonIgnore]
		public string Directory => Path.GetDirectoryName(InfoFile)
			?? throw new InvalidOperationException($"{nameof(InfoFile)} must lead to a directory");
		public int Id { get; set; }
		[JsonIgnore]
		public string InfoFile { get; set; } = null!;
		public string Name { get; set; } = null!;
		public IList<Song> Songs { get; set; }
		public string? Source { get; set; }
		[JsonIgnore]
		public VideoInfo? VideoInfo { get; set; } = null!;
		public int Year { get; set; }
		private string DebuggerDisplay => Name;

		public Anime()
		{
			Songs = new SongCollection(this);
		}

		public Anime(int year, int id, string name) : this()
		{
			Year = year;
			Id = id;
			Name = name;
		}

		public Anime(Anime other) : this(other.Year, other.Id, other.Name)
		{
		}

		public string? GetSourcePath()
			=> FileUtils.GetFile(Directory, Source);

		public void SetSourceFile(string? path)
			=> Source = FileUtils.StoreRelativeOrAbsolute(Directory, path);
	}
}