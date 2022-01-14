using Avalonia.Data.Converters;
using Avalonia.Media;

using SongProcessor.FFmpeg;
using SongProcessor.Models;

namespace SongProcessor.UI.Converters;

public static class MiscConverters
{
	private static readonly IBrush _Cyan = GetBrush(Brushes.Cyan);
	private static readonly IBrush _Green = GetBrush(Brushes.Green);
	private static readonly IBrush _Red = GetBrush(Brushes.Red);
	private static readonly IBrush _Yellow = GetBrush(Brushes.Yellow);

	public static FuncValueConverter<SourceInfo<VideoInfo>?, IBrush?> SourceColor { get; } = new(x =>
		{
			if (x?.File is not string file)
			{
				return _Yellow;
			}
			if (File.Exists(file))
			{
				return Brushes.Transparent;
			}
			return _Red;
		});

	public static FuncValueConverter<Status, IBrush?> StatusColor { get; } = new(x =>
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

	private static SolidColorBrush GetBrush(ISolidColorBrush brush)
		=> new(brush.Color, .25);
}