using Avalonia.Controls;
using Avalonia.Platform.Storage;

using SongProcessor.UI.ViewModels;
using SongProcessor.UI.Views;

namespace SongProcessor.UI;

public sealed class MessageBoxManager(Window window) : IMessageBoxManager
{
	private readonly Window _Window = window;

	public async Task<string?> GetDirectoryAsync(string directory, string title)
	{
		var start = await _Window.StorageProvider.TryGetFolderFromPathAsync(directory).ConfigureAwait(true);
		var result = await _Window.StorageProvider.OpenFolderPickerAsync(new()
		{
			AllowMultiple = false,
			SuggestedStartLocation = start,
			Title = title,
		}).ConfigureAwait(true);

		return result.SingleOrDefault()?.TryGetLocalPath();
	}

	public async Task<string[]> GetFilesAsync(
		string directory,
		string title,
		bool allowMultiple = true)
	{
		var start = await _Window.StorageProvider.TryGetFolderFromPathAsync(directory).ConfigureAwait(true);
		var result = await _Window.StorageProvider.OpenFilePickerAsync(new()
		{
			AllowMultiple = allowMultiple,
			SuggestedStartLocation = start,
			Title = title,
		}).ConfigureAwait(true);

		return result.Select(x =>
		{
			return x.TryGetLocalPath()
				?? throw new InvalidOperationException($"Unable to get URI for a file gathered from {directory}.");
		}).ToArray();
	}

	public Task<T> ShowAsync<T>(MessageBoxViewModel<T> viewModel)
		=> MessageBox.ShowAsync(_Window, viewModel);
}