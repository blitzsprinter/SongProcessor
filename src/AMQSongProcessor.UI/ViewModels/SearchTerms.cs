using System.Linq;
using System.Runtime.Serialization;

using AdvorangesUtils;

using AMQSongProcessor.Models;

using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public sealed class SearchTerms : ReactiveObject
	{
		private string? _AnimeName;
		private string? _ArtistName;
		private string? _SongName;

		[DataMember]
		public string? AnimeName
		{
			get => _AnimeName;
			set => this.RaiseAndSetIfChanged(ref _AnimeName, value);
		}
		[DataMember]
		public string? ArtistName
		{
			get => _ArtistName;
			set => this.RaiseAndSetIfChanged(ref _ArtistName, value);
		}
		[DataMember]
		public string? SongName
		{
			get => _SongName;
			set => this.RaiseAndSetIfChanged(ref _SongName, value);
		}

		public bool IsVisible(IAnime anime)
			=> IsVisible(AnimeName, anime.Name) && anime.Songs.Any(IsVisible);

		public bool IsVisible(ISong song)
			=> IsVisible(SongName, song.Name) && IsVisible(ArtistName, song.Artist);

		private bool IsVisible(string? search, string actual)
			=> string.IsNullOrWhiteSpace(search) || actual.CaseInsContains(search);
	}
}