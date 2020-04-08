﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using AMQSongProcessor.Models;
using AMQSongProcessor.UI.ViewModels;

namespace AMQSongProcessor.UI.Converters
{
	public sealed class SongVisibilityConverter : BaseConverter
	{
		public override int ExpectedValueCount => 2;

		protected override object Convert(ValueCollection values, Type targetType, object parameter, CultureInfo culture)
		{
			var vis = values.ConvertNextValue<SongVisibility>();
			return values.UseNextValue(new IMaybeFunc<bool>[]
			{
				new MaybeFunc<IEnumerable<Song>, bool>(x => x.Any(y => vis.IsVisible(y))),
				new MaybeFunc<Song, bool>(x => vis.IsVisible(x))
			});
		}
	}
}