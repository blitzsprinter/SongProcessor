using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using AMQSongProcessor.Models;

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
			set => RaiseAndSetIfChanged(ref _IsExpanded, value);
		}
		public SongVisibility Self => this;
		[DataMember]
		public bool ShowCompletedSongs
		{
			get => _ShowCompletedSongs;
			set => RaiseAndSetIfChanged(ref _ShowCompletedSongs, value);
		}
		[DataMember]
		public bool ShowIgnoredSongs
		{
			get => _ShowIgnoredSongs;
			set => RaiseAndSetIfChanged(ref _ShowIgnoredSongs, value);
		}
		[DataMember]
		public bool ShowIncompletedSongs
		{
			get => _ShowIncompletedSongs;
			set => RaiseAndSetIfChanged(ref _ShowIncompletedSongs, value);
		}
		[DataMember]
		public bool ShowUnsubmittedSongs
		{
			get => _ShowUnsubmittedSongs;
			set => RaiseAndSetIfChanged(ref _ShowUnsubmittedSongs, value);
		}

		public bool IsVisible(Song song)
		{
			return (ShowIgnoredSongs || !song.ShouldIgnore)
				&& ((ShowCompletedSongs && song.IsCompleted)
				|| (ShowIncompletedSongs && song.IsIncompleted)
				|| (ShowUnsubmittedSongs && song.IsUnsubmitted));
		}

		private void RaiseAndSetIfChanged<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = "")
		{
			IReactiveObjectExtensions.RaiseAndSetIfChanged(this, ref backingField, newValue, propertyName);
			this.RaisePropertyChanged(nameof(Self));
		}
	}
}