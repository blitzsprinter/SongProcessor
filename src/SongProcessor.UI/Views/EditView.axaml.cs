using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public sealed class EditView : ReactiveUserControl<EditViewModel>
{
	public EditView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
		=> AvaloniaXamlLoader.Load(this);
}