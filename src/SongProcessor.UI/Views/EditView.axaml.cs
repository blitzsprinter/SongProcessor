using Avalonia.ReactiveUI;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public partial class EditView : ReactiveUserControl<EditViewModel>
{
	public EditView()
	{
		InitializeComponent();
	}
}