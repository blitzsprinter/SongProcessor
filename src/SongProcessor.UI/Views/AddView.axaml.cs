using Avalonia.ReactiveUI;

using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI.Views;

public partial class AddView : ReactiveUserControl<AddViewModel>
{
	public AddView()
	{
		InitializeComponent();
	}
}