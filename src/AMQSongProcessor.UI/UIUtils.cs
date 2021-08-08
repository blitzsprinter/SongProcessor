using System;
using System.IO;
using System.Threading.Tasks;

using AMQSongProcessor.UI.ViewModels;

namespace AMQSongProcessor.UI
{
	public static class UIUtils
	{
		public static async Task<bool> ConfirmAsync(
			this IMessageBoxManager manager,
			MessageBoxViewModel<string> viewModel)
		{
			viewModel.Options = Constants.YesNo;
			var result = await manager.ShowAsync(viewModel).ConfigureAwait(true);
			return result == Constants.YES;
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
}