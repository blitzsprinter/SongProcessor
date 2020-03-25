using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AMQSongProcessor.Models
{
	public sealed class SongCollection : Collection<Song>
	{
		private readonly Anime _Anime;

		public SongCollection(Anime anime)
		{
			_Anime = anime ?? throw new ArgumentNullException(nameof(anime));
		}

		public SongCollection(Anime anime, IEnumerable<Song> songs) : this(anime)
		{
			foreach (var song in songs)
			{
				Add(song);
			}
		}

		protected override void InsertItem(int index, Song item)
		{
			item.Anime = _Anime;
			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, Song item)
		{
			item.Anime = _Anime;
			base.SetItem(index, item);
		}
	}
}