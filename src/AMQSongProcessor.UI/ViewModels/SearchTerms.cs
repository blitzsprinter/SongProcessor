using System.Runtime.Serialization;

using AdvorangesUtils;

using AMQSongProcessor.Models;
using AMQSongProcessor.UI.Utils;

using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public sealed class SearchTerms : ReactiveObject, IBindableToSelf<SearchTerms>
	{
		private string? _AnimeName;
		private string? _ArtistName;
		private string? _SongName;

		[DataMember]
		public string? AnimeName
		{
			get => _AnimeName;
			set => this.RaiseAndSetIfChangedSelf(ref _AnimeName, value);
		}
		[DataMember]
		public string? ArtistName
		{
			get => _ArtistName;
			set => this.RaiseAndSetIfChangedSelf(ref _ArtistName, value);
		}
		[DataMember]
		public string? SongName
		{
			get => _SongName;
			set => this.RaiseAndSetIfChangedSelf(ref _SongName, value);
		}
		public SearchTerms Self => this;

		object IBindableToSelf.Self => this;

		public bool IsVisible(Anime anime)
		{
			var animeName = Sanitize(AnimeName);
			return animeName == null
				|| anime.Name.CaseInsContains(animeName);
		}

		public bool IsVisible(Song song)
		{
			var songName = Sanitize(SongName);
			var artistName = Sanitize(ArtistName);
			return (songName == null
				&& artistName == null)
				|| song.Name.CaseInsContains(songName)
				|| song.Artist.CaseInsContains(artistName);
		}

		private string? Sanitize(string? value)
			=> string.IsNullOrWhiteSpace(value) ? null : value;
	}
}