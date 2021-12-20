using SongProcessor.UI.ViewModels;

using System.Collections.Immutable;

namespace SongProcessor.UI;

public static class UIUtils
{
	public const int MESSAGE_BOX_HEIGHT = 133;
	public const int MESSAGE_BOX_WIDTH = 278;
	public const string NO = "No";
	public const string YES = "Yes";

	public static ImmutableArray<string> YesNo { get; } = new[] { YES, NO }.ToImmutableArray();

	public static async Task<bool> ConfirmAsync(
		this IMessageBoxManager manager,
		MessageBoxViewModel<string> viewModel)
	{
		viewModel.Options = YesNo;
		var result = await manager.ShowAsync(viewModel).ConfigureAwait(true);
		return result == YES;
	}

	public static Task<string?> GetDirectoryAsync(
		this IMessageBoxManager manager,
		string? directory)
	{
		directory = Directory.Exists(directory) ? directory! : Environment.CurrentDirectory;
		return manager.GetDirectoryAsync(directory, "Directory");
	}

	public static Task ShowNoResultAsync(
		this IMessageBoxManager manager,
		MessageBoxViewModel<object> viewModel)
		=> manager.ShowAsync(viewModel);
}