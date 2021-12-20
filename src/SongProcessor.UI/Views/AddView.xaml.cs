using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public sealed class AddView : ReactiveUserControl<AddViewModel>
{
	public AddView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
		=> AvaloniaXamlLoader.Load(this);
}