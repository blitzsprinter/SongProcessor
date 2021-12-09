
using Avalonia.Data.Converters;

namespace SongProcessor.UI.Converters;

public static class BoolConverters
{
	public static readonly IMultiValueConverter Or =
		new FuncMultiValueConverter<bool, bool>(x => x.Any(y => y));
}
