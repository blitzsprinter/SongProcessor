using Avalonia.Controls;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public partial class MessageBox : Window
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
			// Focusable otherwise Escape keybind doesn't work
			Focusable = true,
		}.ShowDialog<T>(window);
	}
}