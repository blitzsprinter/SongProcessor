#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using AdvorangesUtils;

using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

using ReactiveUI;

namespace AMQSongProcessor.UI.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class ObservableAnime : ReactiveObject, IAnime
	{
		private string _AbsoluteInfoPath;
		private int _Id;
		private string _Name;
		private ObservableCollectionPlus<ObservableSong> _Songs;
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
		public string Name
		{
			get => _Name;
			set => this.RaiseAndSetIfChanged(ref _Name, value);
		}
		public ObservableCollectionPlus<ObservableSong> Songs
		{
			get => _Songs;
			set => this.RaiseAndSetIfChanged(ref _Songs, value);
		}
		public string? Source => FileUtils.StoreRelativeOrAbsolute(this.GetDirectory(), VideoInfo?.Path);
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
		IEnumerable<ISong> IAnimeBase.Songs => Songs;
		private string DebuggerDisplay => Name;

		public ObservableAnime(IAnime anime)
		{
			AbsoluteInfoPath = anime.AbsoluteInfoPath;
			Id = anime.Id;
			Name = anime.Name;
			Songs = new ObservableCollectionPlus<ObservableSong>();
			Songs.AddRange(anime.Songs.Select(x => new ObservableSong(x)));
			VideoInfo = anime.VideoInfo;
			Year = anime.Year;
		}
	}
}