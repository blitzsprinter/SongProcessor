using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public sealed class MainWindow : ReactiveWindow<MainViewModel>
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
		=> AvaloniaXamlLoader.Load(this);
}