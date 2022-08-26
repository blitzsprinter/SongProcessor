using Avalonia.Controls;
using Avalonia.Platform.Storage.FileIO;

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

	public async Task<string?> GetDirectoryAsync(string directory, string title)
	{
		var result = await _Window.StorageProvider.OpenFolderPickerAsync(new()
		{
			AllowMultiple = false,
			SuggestedStartLocation = new BclStorageFolder(directory),
			Title = title,
		}).ConfigureAwait(true);

		return result.SingleOrDefault()?.TryGetUri(out var uri) ?? false
			? uri.LocalPath
			: null;
	}

	public async Task<string[]> GetFilesAsync(
		string directory,
		string title,
		bool allowMultiple = true)
	{
		var result = await _Window.StorageProvider.OpenFilePickerAsync(new()
		{
			AllowMultiple = allowMultiple,
			SuggestedStartLocation = new BclStorageFolder(directory),
			Title = title,
		}).ConfigureAwait(true);

		return result.Select(x =>
		{
			return x.TryGetUri(out var uri)
				? uri.LocalPath
				: throw new InvalidOperationException($"Unable to get URI for a file gathered from {directory}.");
		}).ToArray();
	}

	public Task<T> ShowAsync<T>(MessageBoxViewModel<T> viewModel)
		=> MessageBox.ShowAsync(_Window, viewModel);
}