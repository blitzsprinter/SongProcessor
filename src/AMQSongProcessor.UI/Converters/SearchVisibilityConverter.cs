using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using AMQSongProcessor.Models;
using AMQSongProcessor.UI.ViewModels;

namespace AMQSongProcessor.UI.Converters
{
	public sealed class SearchVisibilityConverter : BaseConverter
	{
		//Fake song is for shows that have no songs so they can show up when no song terms are being searched
		private static readonly ISong _Fake = new Song
		{
			Name = "",
			Artist = "",
		};
		private static readonly IMaybeFunc<SearchTerms, bool>[] _Funcs
			= new MaybeFuncCollectionBuilder<SearchTerms, bool>()
			.Add<ISong>((song, search) => search.IsVisible(song))
			.Add<IAnime>((anime, search) => search.IsVisible(anime))
			.Add<IEnumerable<ISong>>((songs, search) => search.IsVisible(_Fake) || songs.Any(x => search.IsVisible(x)))
			.ToArray();

		public override int ExpectedValueCount => 2;

		protected override object Convert(ValueCollection values, Type targetType, object parameter, CultureInfo culture)
		{
			var search = values.ConvertNextValue<SearchTerms>();
			return values.ConvertNextValue(search, _Funcs);
		}
	}
}