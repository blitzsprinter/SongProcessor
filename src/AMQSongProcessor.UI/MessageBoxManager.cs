using System.Collections.Generic;
using System.Threading.Tasks;

using AMQSongProcessor.UI.Views;

using Avalonia.Controls;

namespace AMQSongProcessor.UI
{
	public sealed class MessageBoxManager : IMessageBoxManager
	{
		private readonly Window _Window;

		public MessageBoxManager(Window window)
		{
			_Window = window;
		}

		public Task<string[]> GetFilesAsync(string directory, string title, bool allowMultiple = true)
		{
			var dialog = new OpenFileDialog
			{
				Directory = directory,
				Title = title,
				AllowMultiple = allowMultiple,
			};
			return dialog.ShowAsync(_Window);
		}

		public Task<T> ShowAsync<T>(string text, string title, IEnumerable<T>? options)
			=> MessageBox.ShowAsync(_Window, text, title, options);
	}
}