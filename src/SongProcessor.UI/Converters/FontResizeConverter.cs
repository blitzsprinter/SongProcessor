using Avalonia.Data.Converters;

using System.Globalization;

namespace SongProcessor.UI.Converters;

public sealed class FontResizeConverter(double convertFactor) : IValueConverter
{
	public double ConvertFactor { get; set; } = convertFactor;

	public FontResizeConverter() : this(.015)
	{
	}

	public object? Convert(object? value, Type _, object? _2, CultureInfo _3)
	{
		if (value is not double dVal)
		{
			throw new InvalidOperationException("Unable to resize font if the passed in value is not a double.");
		}
		if (double.IsNaN(dVal))
		{
			return 1;
		}
		return Math.Max((int)(dVal * ConvertFactor), 1);
	}

	public object? ConvertBack(object? _, Type _2, object? _3, CultureInfo _4)
		=> throw new NotImplementedException();
}