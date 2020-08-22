using System.Collections.Generic;

namespace AMQSongProcessor.Models
{
	public interface IAnimeBase
	{
		public int Id { get; }
		public string Name { get; }
		public IEnumerable<ISong> Songs { get; }
		public string? Source { get; }
		public int Year { get; }
	}
}