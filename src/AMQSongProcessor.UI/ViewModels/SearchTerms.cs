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
		public SearchTerms Self => this;
		[DataMember]
		public string? SongName
		{
			get => _SongName;
			set => this.RaiseAndSetIfChangedSelf(ref _SongName, value);
		}
		object IBindableToSelf.Self => this;

		public bool IsVisible(Anime anime)
			=> IsVisible(AnimeName, anime.Name);

		public bool IsVisible(Song song)
			=> IsVisible(SongName, song.Name) && IsVisible(ArtistName, song.Artist);

		private bool IsVisible(string? search, string actual)
			=> string.IsNullOrWhiteSpace(search) || actual.CaseInsContains(search);
	}
}