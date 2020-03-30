using System.Collections.Generic;
using System.Threading.Tasks;

using AMQSongProcessor.UI.ViewModels;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AMQSongProcessor.UI.Views
{
	public class MessageBox : Window
	{
		public MessageBox()
		{
			InitializeComponent();
		}

		public static Task<T> ShowAsync<T>(
			Window window,
			string text,
			string title,
			IEnumerable<T>? options)
		{
			return new MessageBox
			{
				DataContext = new MessageBoxViewModel
				{
					Text = text,
					Title = title,
					Options = (IEnumerable<object>?)options,
				},
			}.ShowDialog<T>(window);
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}