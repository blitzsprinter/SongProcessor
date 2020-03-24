using AMQSongProcessor.UI.ViewModels;

using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace AMQSongProcessor.UI.Views
{
	public sealed class MainWindow : ReactiveWindow<MainViewModel>
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}