using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public sealed class SongView : ReactiveUserControl<SongViewModel>
{
	public SongView()
	{
		InitializeComponent();
	}

	public void OnKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key is Key.Enter or Key.Return)
		{
			ViewModel?.Load?.Execute();
		}
	}

	private void InitializeComponent()
		=> AvaloniaXamlLoader.Load(this);
}