using Avalonia.Controls;

using SongProcessor.UI.ViewModels;
using SongProcessor.UI.Views;

namespace SongProcessor.UI;

public sealed class MessageBoxManager : IMessageBoxManager
{
	private readonly Window _Window;

	public MessageBoxManager(Window window)
	{
		_Window = window;
	}

	public Task<string?> GetDirectoryAsync(string directory, string title)
	{
		return new OpenFolderDialog
		{
			Directory = directory,
			Title = title,
		}.ShowAsync(_Window);
	}

	public Task<string[]> GetFilesAsync(
		string directory,
		string title,
		bool allowMultiple = true,
		string? initialFileName = null)
	{
		return new OpenFileDialog
		{
			Directory = directory,
			Title = title,
			AllowMultiple = allowMultiple,
			InitialFileName = initialFileName
		}.ShowAsync(_Window);
	}

	public Task<T> ShowAsync<T>(MessageBoxViewModel<T> viewModel)
		=> MessageBox.ShowAsync(_Window, viewModel);
}