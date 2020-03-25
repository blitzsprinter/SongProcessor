using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Anime
	{
		[JsonIgnore]
		public string Directory { get; set; }

		public int Id { get; set; }
		public string Name { get; set; }
		public IList<Song> Songs { get; set; }
		public string Source { get; set; }

		[JsonIgnore]
		public VideoInfo VideoInfo { get; set; }

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

		public string GetCleanSongPath(Song song)
			=> song.CleanPath == null ? null : Path.Combine(Directory, song.CleanPath);

		public string GetSourcePath()
			=> Source == null ? null : Path.Combine(Directory, Source);
	}
}