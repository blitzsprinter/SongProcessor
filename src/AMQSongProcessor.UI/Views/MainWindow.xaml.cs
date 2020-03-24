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
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}