using System.Runtime.Serialization;

using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public sealed class SongVisibility : ReactiveObject
	{
		private bool _IsExpanded;
		private bool _ShowCompletedSongs = true;
		private bool _ShowIgnoredSongs = true;
		private bool _ShowIncompletedSongs = true;
		private bool _ShowUnsubmittedSongs = true;

		[DataMember]
		public bool IsExpanded
		{
			get => _IsExpanded;
			set => this.RaiseAndSetIfChanged(ref _IsExpanded, value);
		}
		[DataMember]
		public bool ShowCompletedSongs
		{
			get => _ShowCompletedSongs;
			set => this.RaiseAndSetIfChanged(ref _ShowCompletedSongs, value);
		}
		[DataMember]
		public bool ShowIgnoredSongs
		{
			get => _ShowIgnoredSongs;
			set => this.RaiseAndSetIfChanged(ref _ShowIgnoredSongs, value);
		}
		[DataMember]
		public bool ShowIncompletedSongs
		{
			get => _ShowIncompletedSongs;
			set => this.RaiseAndSetIfChanged(ref _ShowIncompletedSongs, value);
		}
		[DataMember]
		public bool ShowUnsubmittedSongs
		{
			get => _ShowUnsubmittedSongs;
			set => this.RaiseAndSetIfChanged(ref _ShowUnsubmittedSongs, value);
		}

		public bool IsVisible(ISong song)
		{
			return (ShowIgnoredSongs || !song.ShouldIgnore)
				&& ((ShowCompletedSongs && song.IsCompleted())
				|| (ShowIncompletedSongs && song.IsIncompleted())
				|| (ShowUnsubmittedSongs && song.IsUnsubmitted()));
		}
	}
}