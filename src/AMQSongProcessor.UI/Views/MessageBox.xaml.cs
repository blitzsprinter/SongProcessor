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
			MessageBoxViewModel<T> viewModel)
		{
			return new MessageBox
			{
				DataContext = viewModel,
			}.ShowDialog<T>(window);
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}