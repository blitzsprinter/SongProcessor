using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AnimeModel : IAnimeBase
	{
		public int Id { get; set; }
		public string Name { get; set; } = null!;
		public IList<Song> Songs { get; set; } = new List<Song>();
		public string? Source { get; set; }
		public int Year { get; set; }
		private string DebuggerDisplay => Name;

		public AnimeModel()
		{
		}

		public AnimeModel(IAnimeBase other)
		{
			Id = other.Id;
			Name = other.Name;
			Songs = other.Songs?.ToArray() ?? Array.Empty<Song>();
			Source = other.Source;
			Year = other.Year;
		}
	}
}