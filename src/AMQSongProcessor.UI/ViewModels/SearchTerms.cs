using System.Runtime.Serialization;

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
		{
			// First check if the name of the anime allows it to be shown
			return IsVisible(AnimeName, anime.Name)
				// Then check if any of the songs are allowed to be shown
				&& (anime.Songs.Any(IsVisible)
					// If no songs, make sure we're not searching for any songs/artists
					|| (string.IsNullOrWhiteSpace(SongName)
						&& string.IsNullOrWhiteSpace(ArtistName)
						&& anime.Songs.Count == 0
					)
				);
		}

		public bool IsVisible(ISong song)
			=> IsVisible(SongName, song.Name) && IsVisible(ArtistName, song.Artist);

		private static bool IsVisible(string? search, string actual)
		{
			return string.IsNullOrWhiteSpace(search)
				|| actual.Contains(search!, StringComparison.OrdinalIgnoreCase);
		}
	}
}