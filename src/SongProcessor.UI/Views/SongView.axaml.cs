using Avalonia.Input;
using Avalonia.ReactiveUI;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public partial class SongView : ReactiveUserControl<SongViewModel>
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
}