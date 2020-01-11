using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;

namespace LupinSongsAMQ
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Anime
	{
		[JsonIgnore]
		public string Directory { get; set; }

		public int Id { get; set; }
		public string Name { get; set; }
		public Song[] Songs { get; set; } = Array.Empty<Song>();
		public string Source { get; set; }
		public int Year { get; set; }

		private string DebuggerDisplay => Name;

		public Anime()
		{
		}

		public Anime(int year, int id, string name)
		{
			Year = year;
			Id = id;
			Name = name;
		}

		public string GetSourcePath()
			=> Source == null ? null : Path.Combine(Directory, Source);
	}
}