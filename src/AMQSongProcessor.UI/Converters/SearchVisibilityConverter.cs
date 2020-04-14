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
		private static readonly IMaybeFunc<SearchTerms, bool>[] _Funcs
			= new MaybeFuncCollectionBuilder<SearchTerms, bool>()
			.Add<Song>((song, search) => search.IsVisible(song))
			.Add<Anime>((anime, search) => search.IsVisible(anime))
			.Add<IEnumerable<Song>>((songs, search) => songs.Any(x => search.IsVisible(x)))
			.ToArray();

		public override int ExpectedValueCount => 2;

		protected override object Convert(ValueCollection values, Type targetType, object parameter, CultureInfo culture)
		{
			var search = values.ConvertNextValue<SearchTerms>();
			return values.ConvertNextValue(search, _Funcs);
		}
	}
}