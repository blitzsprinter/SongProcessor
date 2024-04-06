using Avalonia.ReactiveUI;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
	public MainWindow()
	{
		InitializeComponent();
	}
}