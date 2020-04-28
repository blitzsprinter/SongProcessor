using System.Collections.Generic;
using System.Threading.Tasks;

namespace AMQSongProcessor.UI
{
	public interface IMessageBoxManager
	{
		Task<string?> GetDirectoryAsync(string directory, string title);

		Task<string[]> GetFilesAsync(string directory, string title, bool allowMultiple = true, string? initialFileName = null);

		Task<T> ShowAsync<T>(string text, string title, IEnumerable<T>? options);
	}

	public static class MessageBoxManagerUtils
	{
		public static Task ShowAsync(this IMessageBoxManager manager, string text, string title)
			=> manager.ShowAsync<object>(text, title, null);
	}
}