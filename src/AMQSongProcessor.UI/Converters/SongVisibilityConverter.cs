using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using AMQSongProcessor.Models;
using AMQSongProcessor.UI.ViewModels;

using Avalonia.Data.Converters;

namespace AMQSongProcessor.UI.Converters
{
	public sealed class SongVisibilityConverter : IMultiValueConverter
	{
		public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Count != 2)
			{
				throw new ArgumentException("Invalid value count. Expected 2.");
			}
			if (!(values[0] is SongVisibility songVisibility))
			{
				throw new InvalidCastException($"Invalid first value. Expected {nameof(SongVisibility)}.");
			}
			if (values[1] is IEnumerable<Song> songs)
			{
				return songs.Any(x => songVisibility.IsVisible(x));
			}
			if (values[1] is Song song)
			{
				return songVisibility.IsVisible(song);
			}
			throw new InvalidCastException($"Invalid second value. Expected {nameof(Song)}.");
		}
	}
}