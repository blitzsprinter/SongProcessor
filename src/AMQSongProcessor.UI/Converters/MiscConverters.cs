using AMQSongProcessor.Models;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AMQSongProcessor.UI.Converters
{
	public static class MiscConverters
	{
		public static readonly IMultiValueConverter SongVisibility =
			new SongVisibilityConverter();
		public static readonly IValueConverter StatusColor =
			new FuncValueConverter<Status, IBrush>(x =>
			{
				const Status ANY_VIDEO = Status.Res480 | Status.Res720;

				if ((x & ANY_VIDEO) != 0)
				{
					return _Green;
				}
				if ((x & Status.None) != 0)
				{
					return _Yellow;
				}
				return _Red;
			});

		private static readonly SolidColorBrush _Green = GetBrush(Brushes.Green);
		private static readonly SolidColorBrush _Red = GetBrush(Brushes.Red);
		private static readonly SolidColorBrush _Yellow = GetBrush(Brushes.Yellow);

		private static SolidColorBrush GetBrush(ISolidColorBrush brush)
			=> new SolidColorBrush(brush.Color, .25);
	}
}