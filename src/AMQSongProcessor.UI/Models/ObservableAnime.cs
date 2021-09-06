using System.Collections.ObjectModel;
using System.Diagnostics;

using AMQSongProcessor.FFmpeg;
using AMQSongProcessor.Models;

using ReactiveUI;

namespace AMQSongProcessor.UI.Models
{
	[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
	public sealed class ObservableAnime : ReactiveObject, IAnime
	{
		private string _AbsoluteInfoPath = null!;
		private int _Id;
		private bool _IsExpanded;
		private bool _IsExpanderVisible;
		private bool _IsVisible = true;
		private string _Name = null!;
		private ObservableCollection<ObservableSong> _Songs = null!;
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
			Songs = new SortedObservableCollection<ObservableSong>(SongComparer.Instance, songs);
			IsExpanderVisible = Songs.Count > 0;
			VideoInfo = anime.VideoInfo;
			Year = anime.Year;
		}
	}
}