using AMQSongProcessor.UI.ViewModels;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace AMQSongProcessor.UI.Views
{
	public sealed class SongView : ReactiveUserControl<SongViewModel>
	{
		public SongView()
		{
			InitializeComponent();
		}

		public void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				if (ViewModel.Load.CanExecute(null))
				{
					ViewModel.Load.Execute(null);
				}
			}
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}