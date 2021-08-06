using System.IO;

using AMQSongProcessor.Models;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AMQSongProcessor.UI.Converters
{
	public static class MiscConverters
	{
		public static readonly IValueConverter SourceColor =
			new FuncValueConverter<SourceInfo<VideoInfo>?, IBrush?>(x =>
			{
				if (x?.Path is not string path)
				{
					return _Yellow;
				}
				if (File.Exists(path))
				{
					return Brushes.Transparent;
				}
				return _Red;
			});
		public static readonly IValueConverter StatusColor =
			new FuncValueConverter<Status, IBrush?>(x =>
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
			=> new(brush.Color, .25);
	}
}