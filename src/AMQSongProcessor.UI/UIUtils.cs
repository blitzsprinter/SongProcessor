using System;
using System.IO;
using System.Threading.Tasks;

namespace AMQSongProcessor.UI
{
	public static class UIUtils
	{
		public static async Task<bool> ConfirmAsync(
			this IMessageBoxManager manager,
			string text,
			string title)
		{
			var result = await manager.ShowAsync(text, title, Constants.YesNo).ConfigureAwait(true);
			return result == Constants.YES;
		}

		public static Task<string?> GetDirectoryAsync(
			this IMessageBoxManager manager,
			string? directory)
		{
			directory = Directory.Exists(directory) ? directory! : Environment.CurrentDirectory;
			return manager.GetDirectoryAsync(directory, "Directory");
		}

		public static Task ShowAsync(
			this IMessageBoxManager manager,
			string text,
			string title)
			=> manager.ShowAsync<object>(text, title, null);
	}
}