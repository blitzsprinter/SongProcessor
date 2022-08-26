using SongProcessor.UI.ViewModels;

namespace SongProcessor.UI;

public interface IMessageBoxManager
{
	Task<string?> GetDirectoryAsync(string directory, string title);

	Task<string[]> GetFilesAsync(string directory, string title, bool allowMultiple = true);

	Task<T> ShowAsync<T>(MessageBoxViewModel<T> viewModel);
}