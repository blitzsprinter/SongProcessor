using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AnimeModel : IAnimeBase
	{
		public int Id { get; set; }
		public string Name { get; set; } = null!;
		public List<Song> Songs { get; set; } = new List<Song>();
		public string? Source { get; set; }
		public int Year { get; set; }
		IReadOnlyList<ISong> IAnimeBase.Songs => Songs;
		private string DebuggerDisplay => Name;

		public AnimeModel()
		{
		}

		public AnimeModel(IAnimeBase other)
		{
			Id = other.Id;
			Name = other.Name;
			Songs = other.Songs?.Select(x => new Song(x))?.ToList() ?? new List<Song>();
			Source = other.Source;
			Year = other.Year;
		}
	}
}