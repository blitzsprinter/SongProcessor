using System.Collections.Generic;
using System.Linq;

using AMQSongProcessor.Models;

using Avalonia.Data.Converters;

namespace AMQSongProcessor.UI.Converters
{
	public static class MiscConverters
	{
		public static readonly IValueConverter IgnoredSongs =
			new FuncValueConverter<IEnumerable<Song>, bool>(x => x.Any(y => !y.ShouldIgnore));
	}
}