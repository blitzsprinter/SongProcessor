using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public sealed class MessageBox : Window
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

	private void InitializeComponent()
		=> AvaloniaXamlLoader.Load(this);
}