using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using AMQSongProcessor.Models;
using AMQSongProcessor.UI.ViewModels;

namespace AMQSongProcessor.UI.Converters
{
	public sealed class SongVisibilityConverter : BaseConverter
	{
		private static readonly IMaybeFunc<SongVisibility, bool>[] _Funcs
			= new MaybeFuncCollectionBuilder<SongVisibility, bool>()
			.Add<Song>((song, vis) => vis.IsVisible(song))
			.Add<IEnumerable<Song>>((songs, vis) => songs.Any(x => vis.IsVisible(x)))
			.ToArray();

		public override int ExpectedValueCount => 2;

		protected override object Convert(ValueCollection values, Type targetType, object parameter, CultureInfo culture)
		{
			var vis = values.ConvertNextValue<SongVisibility>();
			return values.ConvertNextValue(vis, _Funcs);
		}
	}
}