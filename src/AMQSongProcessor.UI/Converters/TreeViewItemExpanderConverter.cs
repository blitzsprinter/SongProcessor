using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using AMQSongProcessor.Models;
using AMQSongProcessor.UI.ViewModels;

using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace AMQSongProcessor.UI.Converters
{
	public sealed class TreeViewItemExpanderConverter : IMultiValueConverter
	{
		public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				var vm = (SongViewModel)values[0];
				var songs = (IEnumerable<Song>)values[1];

				return songs.Any(x =>
				{
					return (vm.ShowIgnoredSongs || !x.ShouldIgnore)
						&& ((vm.ShowCompletedSongs && x.IsCompleted)
						|| (vm.ShowIncompletedSongs && x.IsIncompleted)
						|| (vm.ShowUnsubmittedSongs && x.IsUnsubmitted));
				});
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Unable to determine if {nameof(TreeViewItem)} should have its expander visible.", e);
			}
		}
	}
}