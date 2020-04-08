using System.IO;

using AMQSongProcessor.Models;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AMQSongProcessor.UI.Converters
{
	public static class MiscConverters
	{
		public static readonly IValueConverter SourceColor =
			new FuncValueConverter<string, IBrush>(x =>
			{
				if (File.Exists(x))
				{
					return Brushes.Transparent;
				}
				if (string.IsNullOrWhiteSpace(x))
				{
					return _Yellow;
				}
				return _Red;
			});
		public static readonly IValueConverter StatusColor =
			new FuncValueConverter<Status, IBrush>(x =>
			{
				if ((x & Status.Res720) != 0)
				{
					return _Green;
				}
				if ((x & Status.Res480) != 0)
				{
					return _Cyan;
				}
				if ((x & Status.None) != 0)
				{
					return _Yellow;
				}
				return _Red;
			});

		private static readonly SolidColorBrush _Cyan = GetBrush(Brushes.Cyan);
		private static readonly SolidColorBrush _Green = GetBrush(Brushes.Green);
		private static readonly SolidColorBrush _Red = GetBrush(Brushes.Red);
		private static readonly SolidColorBrush _Yellow = GetBrush(Brushes.Yellow);

		private static SolidColorBrush GetBrush(ISolidColorBrush brush)
			=> new SolidColorBrush(brush.Color, .25);
	}
}