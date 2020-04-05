using System.Collections.Generic;
using System.Linq;

using AMQSongProcessor.Models;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AMQSongProcessor.UI.Converters
{
	public static class MiscConverters
	{
		public static readonly IValueConverter AnyUnignoredSongs =
			new FuncValueConverter<IEnumerable<Song>, bool>(x => x.Any(y => !y.ShouldIgnore));

		public static readonly IValueConverter StatusColor =
			new FuncValueConverter<Status, IBrush>(x =>
			{
				const Status ANY_VIDEO = Status.Res480 | Status.Res720;

				if ((x & ANY_VIDEO) != 0)
				{
					return Green;
				}
				if ((x & Status.None) != 0)
				{
					return Yellow;
				}
				return Red;
			});

		private static readonly SolidColorBrush Green = GetBrush(Brushes.Green);
		private static readonly SolidColorBrush Red = GetBrush(Brushes.Red);
		private static readonly SolidColorBrush Yellow = GetBrush(Brushes.Yellow);

		private static SolidColorBrush GetBrush(ISolidColorBrush brush)
			=> new SolidColorBrush(brush.Color, .25);
	}
}