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
		public IList<ISong> Songs { get; set; } = new List<ISong>();
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
			Songs = other.Songs?.Select(x => new Song(x))?.ToList<ISong>() ?? new List<ISong>();
			Source = other.Source;
			Year = other.Year;
		}
	}
}