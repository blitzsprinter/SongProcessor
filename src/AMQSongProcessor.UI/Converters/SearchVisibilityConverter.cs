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
		public override int ExpectedValueCount => 2;

		protected override object Convert(ValueCollection values, Type targetType, object parameter, CultureInfo culture)
		{
			var search = values.ConvertNextValue<SearchTerms>();
			return values.UseNextValue(new IMaybeFunc<bool>[]
			{
				new MaybeFunc<Anime, bool>(x => search.IsVisible(x)),
				new MaybeFunc<IEnumerable<Song>, bool>(x => x.Any(y => search.IsVisible(y))),
				new MaybeFunc<Song, bool>(x => search.IsVisible(x))
			});
		}
	}
}