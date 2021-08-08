#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

using ReactiveUI;

namespace AMQSongProcessor.UI.Models
{
	[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
	public sealed class ObservableAnime : ReactiveObject, IAnime
	{
		private static readonly SongComparer _SongComparer = new();

		private string _AbsoluteInfoPath;
		private int _Id;
		private bool _IsExpanded;
		private bool _IsExpanderVisible;
		private bool _IsVisible = true;
		private string _Name;
		private ObservableCollection<ObservableSong> _Songs;
		private SourceInfo<VideoInfo>? _VideoInfo;
		private int _Year;

		public string AbsoluteInfoPath
		{
			get => _AbsoluteInfoPath;
			set => this.RaiseAndSetIfChanged(ref _AbsoluteInfoPath, value);
		}
		public int Id
		{
			get => _Id;
			set => this.RaiseAndSetIfChanged(ref _Id, value);
		}
		public bool IsExpanded
		{
			get => _IsExpanded;
			set => this.RaiseAndSetIfChanged(ref _IsExpanded, value);
		}
		public bool IsExpanderVisible
		{
			get => _IsExpanderVisible;
			set => this.RaiseAndSetIfChanged(ref _IsExpanderVisible, value);
		}
		public bool IsVisible
		{
			get => _IsVisible;
			set => this.RaiseAndSetIfChanged(ref _IsVisible, value);
		}
		public string Name
		{
			get => _Name;
			set => this.RaiseAndSetIfChanged(ref _Name, value);
		}
		public ObservableCollection<ObservableSong> Songs
		{
			get => _Songs;
			set => this.RaiseAndSetIfChanged(ref _Songs, value);
		}
		public string? Source => this.GetRelativeOrAbsoluteSourcePath();
		public SourceInfo<VideoInfo>? VideoInfo
		{
			get => _VideoInfo;
			set
			{
				this.RaiseAndSetIfChanged(ref _VideoInfo, value);
				this.RaisePropertyChanged(nameof(Source));
			}
		}
		public int Year
		{
			get => _Year;
			set => this.RaiseAndSetIfChanged(ref _Year, value);
		}
		IReadOnlyList<ISong> IAnimeBase.Songs => Songs;
		private string DebuggerDisplay => Name;

		public ObservableAnime(IAnime anime)
		{
			AbsoluteInfoPath = anime.AbsoluteInfoPath;
			Id = anime.Id;
			Name = anime.Name;
			var songs = anime.Songs.Select(x => new ObservableSong(this, x));
			Songs = new SortedObservableCollection<ObservableSong>(_SongComparer, songs);
			IsExpanderVisible = Songs.Count > 0;
			VideoInfo = anime.VideoInfo;
			Year = anime.Year;
		}
	}
}