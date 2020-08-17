using System.Collections.Generic;

namespace AMQSongProcessor.Models
{
	public interface IAnimeBase
	{
		public int Id { get; }
		public string Name { get; }
		public IList<Song> Songs { get; }
		public string? Source { get; }
		public int Year { get; }
	}
}