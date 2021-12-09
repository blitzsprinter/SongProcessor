using SongProcessor.UI.ViewModels;

using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

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
