using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AMQSongProcessor.Models
{
	public sealed class SongCollection : ObservableCollection<Song>
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

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Items));
			base.OnCollectionChanged(e);
		}

		protected override void SetItem(int index, Song item)
		{
			item.Anime = _Anime;
			base.SetItem(index, item);
		}

		private void OnPropertyChanged([CallerMemberName] string name = "")
			=> OnPropertyChanged(new PropertyChangedEventArgs(name));
	}
}