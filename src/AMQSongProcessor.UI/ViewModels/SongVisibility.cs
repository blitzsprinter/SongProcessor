using System.Runtime.Serialization;

using AMQSongProcessor.Models;
using AMQSongProcessor.UI.Utils;

using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public sealed class SongVisibility : ReactiveObject, IBindableToSelf<SongVisibility>
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
			set => this.RaiseAndSetIfChangedSelf(ref _IsExpanded, value);
		}
		public SongVisibility Self => this;
		[DataMember]
		public bool ShowCompletedSongs
		{
			get => _ShowCompletedSongs;
			set => this.RaiseAndSetIfChangedSelf(ref _ShowCompletedSongs, value);
		}
		[DataMember]
		public bool ShowIgnoredSongs
		{
			get => _ShowIgnoredSongs;
			set => this.RaiseAndSetIfChangedSelf(ref _ShowIgnoredSongs, value);
		}
		[DataMember]
		public bool ShowIncompletedSongs
		{
			get => _ShowIncompletedSongs;
			set => this.RaiseAndSetIfChangedSelf(ref _ShowIncompletedSongs, value);
		}
		[DataMember]
		public bool ShowUnsubmittedSongs
		{
			get => _ShowUnsubmittedSongs;
			set => this.RaiseAndSetIfChangedSelf(ref _ShowUnsubmittedSongs, value);
		}

		object IBindableToSelf.Self => this;

		public bool IsVisible(Song song)
		{
			return (ShowIgnoredSongs || !song.ShouldIgnore)
				&& ((ShowCompletedSongs && song.IsCompleted)
				|| (ShowIncompletedSongs && song.IsIncompleted)
				|| (ShowUnsubmittedSongs && song.IsUnsubmitted));
		}
	}
}