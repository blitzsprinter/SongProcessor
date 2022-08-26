using Avalonia.Collections;
using Avalonia.Input.Platform;
using Avalonia.Threading;

using DynamicData;
using DynamicData.Binding;

using ReactiveUI;

using SongProcessor.FFmpeg;
using SongProcessor.Models;
using SongProcessor.UI.Models;
using SongProcessor.Utils;

using Splat;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;

namespace SongProcessor.UI.ViewModels;

[DataContract]
public sealed class SongViewModel : ReactiveObject, IRoutableViewModel, INavigationController
{
	private static readonly SaveNewOptions _SaveOptions = new
	(
		AddShowNameDirectory: false,
		AllowOverwrite: false,
		CreateDuplicateFile: true
	);

	private readonly ISourceInfoGatherer _Gatherer;
	private readonly ObservableAsPropertyHelper<bool> _IsBusy;
	private readonly ObservableAsPropertyHelper<bool> _IsProcessing;
	private readonly ISongLoader _Loader;
	private readonly IMessageBoxManager _MessageBoxManager;
	private readonly ObservableAsPropertyHelper<bool> _MultipleItemsSelected;
	private readonly ObservableAsPropertyHelper<bool> _OnlySongsSelected;
	private readonly ISongProcessor _Processor;
	private readonly List<IDisposable> _Subscriptions = new();
	private readonly IClipboard _SystemClipboard;
	private Clipboard<ObservableSong>? _ClipboardSong;
	private int _CurrentJob;
	private string? _Directory;
	private ProcessingData? _ProcessingData;
	private int _QueuedJobs;
	private SearchTerms _Search = new();
	private AvaloniaList<object> _SelectedItems = new();
	private SongVisibility _SongVisibility = new();

	public ObservableCollection<ObservableAnime> Anime { get; } =
		new SortedObservableCollection<ObservableAnime>(AnimeComparer.Instance);
	public IObservable<bool> CanNavigate { get; }
	public Clipboard<ObservableSong>? ClipboardSong
	{
		get => _ClipboardSong;
		set => this.RaiseAndSetIfChanged(ref _ClipboardSong, value);
	}
	public int CurrentJob
	{
		get => _CurrentJob;
		set => this.RaiseAndSetIfChanged(ref _CurrentJob, value);
	}
	[DataMember]
	public string? Directory
	{
		get => _Directory;
		set => this.RaiseAndSetIfChanged(ref _Directory, value);
	}
	public IScreen HostScreen { get; }
	public bool IsBusy => _IsBusy.Value;
	public bool IsProcessing => _IsProcessing.Value;
	public bool MultipleItemsSelected => _MultipleItemsSelected.Value;
	public bool OnlySongsSelected => _OnlySongsSelected.Value;
	public ProcessingData? ProcessingData
	{
		get => _ProcessingData;
		set => this.RaiseAndSetIfChanged(ref _ProcessingData, value);
	}
	public int QueuedJobs
	{
		get => _QueuedJobs;
		set => this.RaiseAndSetIfChanged(ref _QueuedJobs, value);
	}
	[DataMember]
	public SearchTerms Search
	{
		get => _Search;
		set => this.RaiseAndSetIfChanged(ref _Search, value);
	}
	public AvaloniaList<object> SelectedItems
	{
		get => _SelectedItems;
		set => this.RaiseAndSetIfChanged(ref _SelectedItems, value);
	}
	[DataMember]
	public SongVisibility SongVisibility
	{
		get => _SongVisibility;
		set => this.RaiseAndSetIfChanged(ref _SongVisibility, value);
	}
	public string UrlPathSegment => "/songs";

	#region Commands
	public ReactiveCommand<ObservableAnime, Unit> AddSong { get; }
	public ReactiveCommand<Unit, Unit> CancelProcessing { get; }
	public ReactiveCommand<ObservableAnime, Unit> ChangeSource { get; }
	public ReactiveCommand<ObservableAnime, Unit> ClearSongs { get; }
	public ReactiveCommand<ObservableAnime, Unit> ClearSource { get; }
	public ReactiveCommand<int, Unit> CopyANNID { get; }
	public ReactiveCommand<ObservableSong, Unit> CopySong { get; }
	public ReactiveCommand<ObservableSong, Unit> CutSong { get; }
	public ReactiveCommand<ObservableAnime, Unit> DeleteAnime { get; }
	public ReactiveCommand<ObservableSong, Unit> DeleteSong { get; }
	public ReactiveCommand<ObservableAnime, Unit> DuplicateAnime { get; }
	public ReactiveCommand<ObservableSong, Unit> EditSong { get; }
	public ReactiveCommand<Unit, Unit> ExportFixes { get; }
	public ReactiveCommand<ObservableAnime, Unit> GetVolumeInfo { get; }
	public ReactiveCommand<Unit, Unit> Load { get; }
	public ReactiveCommand<StatusModifier, Unit> ModifyMultipleSongsStatus { get; }
	public ReactiveCommand<ObservableAnime, Unit> OpenInfoFile { get; }
	public ReactiveCommand<ObservableAnime, Unit> PasteSong { get; }
	public ReactiveCommand<Unit, Unit> ProcessSongs { get; }
	public ReactiveCommand<Unit, Unit> SelectDirectory { get; }
	public ReactiveCommand<Unit, Unit> Unload { get; }
	#endregion Commands

	public SongViewModel(
		IScreen screen,
		ISongLoader loader,
		ISongProcessor processor,
		ISourceInfoGatherer gatherer,
		IClipboard clipboard,
		IMessageBoxManager messageBoxManager)
	{
		HostScreen = screen ?? throw new ArgumentNullException(nameof(screen));
		_Loader = loader ?? throw new ArgumentNullException(nameof(loader));
		_Processor = processor ?? throw new ArgumentNullException(nameof(processor));
		_Gatherer = gatherer ?? throw new ArgumentNullException(nameof(gatherer));
		_SystemClipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
		_MessageBoxManager = messageBoxManager ?? throw new ArgumentNullException(nameof(messageBoxManager));

		Unload = ReactiveCommand.CreateFromTask(UnloadAsync);
		CopyANNID = ReactiveCommand.CreateFromTask<int>(CopyANNIDAsync);
		OpenInfoFile = ReactiveCommand.CreateFromTask<ObservableAnime>(OpenInfoFileAsync);
		GetVolumeInfo = ReactiveCommand.CreateFromTask<ObservableAnime>(GetVolumeInfoAsync);
		DuplicateAnime = ReactiveCommand.CreateFromTask<ObservableAnime>(DuplicateAnimeAsync);
		DeleteAnime = ReactiveCommand.CreateFromTask<ObservableAnime>(DeleteAnimeAsync);
		ClearSongs = ReactiveCommand.CreateFromTask<ObservableAnime>(ClearSongsAsync);
		ChangeSource = ReactiveCommand.CreateFromTask<ObservableAnime>(ChangeSourceAsync);
		ClearSource = ReactiveCommand.CreateFromTask<ObservableAnime>(ClearSourceAsync);
		AddSong = ReactiveCommand.CreateFromTask<ObservableAnime>(AddSongAsync);
		EditSong = ReactiveCommand.CreateFromTask<ObservableSong>(EditSongAsync);
		CopySong = ReactiveCommand.CreateFromTask<ObservableSong>(CopySongAsync);
		CutSong = ReactiveCommand.CreateFromTask<ObservableSong>(CutSongAsync);
		DeleteSong = ReactiveCommand.CreateFromTask<ObservableSong>(DeleteSongAsync);
		ExportFixes = ReactiveCommand.CreateFromTask(ExportFixesAsync);
		ProcessSongs = ReactiveCommand.CreateFromObservable(ProcessSongsObservable);
		CancelProcessing = ReactiveCommand.Create(() => { });
		SelectDirectory = ReactiveCommand.CreateFromTask(SelectDirectoryAsync);
		ModifyMultipleSongsStatus = ReactiveCommand.CreateFromTask<StatusModifier>(ModifyMultipleSongsStatusAsync);

		var validDirectory = this
			.WhenAnyValue(x => x.Directory)
			.Select(System.IO.Directory.Exists);
		Load = ReactiveCommand.CreateFromTask(LoadAsync, validDirectory);

		var validClipboard = this
			.WhenAnyValue(x => x.ClipboardSong)
			.Select(x => x is not null);
		PasteSong = ReactiveCommand.CreateFromTask<ObservableAnime>(PasteSongAsync, validClipboard);

		var loading = Load.IsExecuting;
		var processing = ProcessSongs.IsExecuting;
		var busy = loading.CombineLatest(processing, (x, y) => x || y);

		_IsProcessing = processing.ToProperty(this, x => x.IsProcessing);
		_IsBusy = busy.ToProperty(this, x => x.IsBusy);

		var multiple = this
			.WhenAnyValue(x => x.SelectedItems.Count)
			.Select(x => x > 1);
		_MultipleItemsSelected = multiple.ToProperty(this, x => x.MultipleItemsSelected);

		var onlySongs = this
			.WhenAnyValue(x => x.SelectedItems.Count)
			.Select(x => x == 0 || SelectedItems.All(y => y is ISong));
		_OnlySongsSelected = onlySongs.ToProperty(this, x => x.OnlySongsSelected);

		var loaded = this
			.WhenAnyValue(x => x.Anime.Count)
			.Select(x => x != 0);
		CanNavigate = busy.CombineLatest(loaded, (x, y) => !(x || y));
	}

	private SongViewModel() : this(
		Locator.Current.GetService<IScreen>()!,
		Locator.Current.GetService<ISongLoader>()!,
		Locator.Current.GetService<ISongProcessor>()!,
		Locator.Current.GetService<ISourceInfoGatherer>()!,
		Locator.Current.GetService<IClipboard>()!,
		Locator.Current.GetService<IMessageBoxManager>()!)
	{
	}

	private async Task AddSongAsync(ObservableAnime anime)
	{
		var song = new ObservableSong(anime, new Song());
		anime.Songs.Add(song);
		await HostScreen.Router.Navigate.Execute(new EditViewModel(
			HostScreen,
			_Loader,
			_MessageBoxManager,
			song
		));
	}

	private async Task ChangeSourceAsync(ObservableAnime anime)
	{
		var dir = anime.GetDirectory();
		var result = await _MessageBoxManager.GetFilesAsync(dir, "Source", false).ConfigureAwait(true);
		if (result.SingleOrDefault() is not string file)
		{
			return;
		}

		try
		{
			anime.VideoInfo = await _Gatherer.GetVideoInfoAsync(file).ConfigureAwait(true);
		}
		catch (Exception)
		{
			await Dispatcher.UIThread.InvokeAsync(() =>
			{
				return _MessageBoxManager.ShowNoResultAsync(new()
				{
					Text = $"\"{file}\" is an invalid file for a video source.",
					Title = "Invalid File",
				});
			}).ConfigureAwait(true);
			return;
		}

		await _Loader.SaveAsync(anime).ConfigureAwait(true);
	}

	private async Task ClearSongsAsync(ObservableAnime anime)
	{
		var result = await _MessageBoxManager.ConfirmAsync(new()
		{
			Text = $"Are you sure you want to delete all songs from {anime.Name}?",
			Title = "Song Clearing",
		}).ConfigureAwait(true);
		if (!result)
		{
			return;
		}

		anime.Songs.Clear();
		await _Loader.SaveAsync(anime).ConfigureAwait(true);
	}

	private async Task ClearSourceAsync(ObservableAnime anime)
	{
		anime.VideoInfo = null;
		await _Loader.SaveAsync(anime).ConfigureAwait(true);
	}

	private Task CopyANNIDAsync(int id)
		=> _SystemClipboard.SetTextAsync(id.ToString());

	private Task CopySongAsync(ObservableSong song)
	{
		ClipboardSong = new(song, false, null);
		return Task.CompletedTask;
	}

	private Task CutSongAsync(ObservableSong song)
	{
		ClipboardSong = new(song, true, () =>
		{
			var anime = song.Parent;
			anime.Songs.Remove(song);
			return _Loader.SaveAsync(anime);
		});
		return Task.CompletedTask;
	}

	private async Task DeleteAnimeAsync(ObservableAnime anime)
	{
		var result = await _MessageBoxManager.ConfirmAsync(new()
		{
			Text = $"Are you sure you want to delete {anime.Name}?",
			Title = "Anime Deletion",
		}).ConfigureAwait(true);
		if (!result)
		{
			return;
		}

		Anime.Remove(anime);
		File.Delete(anime.AbsoluteInfoPath);
	}

	private async Task DeleteSongAsync(ObservableSong song)
	{
		var anime = song.Parent;
		var result = await _MessageBoxManager.ConfirmAsync(new()
		{
			Text = $"Are you sure you want to delete \"{song.Name}\" from {anime.Name}?",
			Title = "Song Deletion",
		}).ConfigureAwait(true);
		if (!result)
		{
			return;
		}

		anime.Songs.Remove(song);
		await _Loader.SaveAsync(anime).ConfigureAwait(true);
	}

	private async Task DuplicateAnimeAsync(ObservableAnime anime)
	{
		var file = await _Loader.SaveNewAsync(anime.GetDirectory(), anime, _SaveOptions).ConfigureAwait(true);
		Anime.Add(new ObservableAnime(new Anime(file!, anime, anime.VideoInfo)));
	}

	private async Task EditSongAsync(ObservableSong song)
	{
		await HostScreen.Router.Navigate.Execute(new EditViewModel(
			HostScreen,
			_Loader,
			_MessageBoxManager,
			song
		));
	}

	private Task ExportFixesAsync()
		=> _Processor.ExportFixesAsync(Anime, Directory!);

	private async Task GetVolumeInfoAsync(ObservableAnime anime)
	{
		var dir = anime.GetDirectory();
		var paths = await _MessageBoxManager.GetFilesAsync(dir, "Volume Info", true).ConfigureAwait(true);
		if (paths.Length == 0)
		{
			return;
		}

		foreach (var path in paths)
		{
			VolumeInfo info;
			try
			{
				info = await _Gatherer.GetVolumeInfoAsync(path).ConfigureAwait(true);
			}
			catch (Exception e)
			{
				_ = Dispatcher.UIThread.InvokeAsync(
					() => _MessageBoxManager.ShowExceptionAsync(e)
				).ConfigureAwait(true);
				continue;
			}

			var text = $"Volume information for \"{Path.GetFileName(path)}\":" +
				$"\nMean volume: {info.MeanVolume}dB" +
				$"\nMax volume: {info.MaxVolume}dB";
			_ = Dispatcher.UIThread.InvokeAsync(() =>
			{
				return _MessageBoxManager.ShowNoResultAsync(new()
				{
					Text = text,
					Title = "Volume Info",
					Width = UIUtils.MESSAGE_BOX_WIDTH + path.Length,
				});
			}).ConfigureAwait(true);
		}
	}

	private async Task LoadAsync()
	{
		try
		{
			var files = _Loader.GetFiles(Directory!);
			await foreach (var item in _Loader.LoadFromFilesAsync(files, 3))
			{
				var anime = new ObservableAnime(item);
				ModifyVisibility(anime);
				Anime.Add(anime);

				_Subscriptions.Add(anime.Changed
					.Subscribe(_ => ModifyVisibility(anime))
				);
				_Subscriptions.Add(anime.Songs
					.ToObservableChangeSet()
					.WhenAnyPropertyChanged()
					.Subscribe(_ => ModifyVisibility(anime))
				);
			}

			_Subscriptions.Add(Search.Changed
				.Merge(SongVisibility.Changed)
				.Subscribe(_ =>
				{
					foreach (var anime in Anime)
					{
						ModifyVisibility(anime);
					}
				})
			);
		}
		catch (Exception e)
		{
			await _MessageBoxManager.ShowExceptionAsync(e).ConfigureAwait(false);
		}
	}

	private async Task ModifyMultipleSongsStatusAsync(StatusModifier modifier)
	{
		var isRemove = modifier < 0;
		var status = modifier.ToStatus();

		var result = await _MessageBoxManager.ConfirmAsync(new()
		{
			Text = $"Are you sure you want to modify {SelectedItems.Count} " +
				$"songs status' by {(isRemove ? "removing" : "adding")} {status}?",
			Title = "Multiple Song Status Modification",
		}).ConfigureAwait(true);
		if (!result)
		{
			return;
		}

		foreach (var group in SelectedItems.OfType<ObservableSong>().GroupBy(x => x.Parent))
		{
			foreach (var song in group)
			{
				if (isRemove)
				{
					song.Status &= ~status;
				}
				else
				{
					song.Status |= status;
				}
			}

			var anime = group.Key;
			await _Loader.SaveAsync(anime).ConfigureAwait(true);
		}
	}

	private void ModifyVisibility(ObservableAnime anime)
	{
		var isExpanderVisible = false;
		foreach (var song in anime.Songs)
		{
			song.IsVisible = Search.IsVisible(song) && SongVisibility.IsVisible(song);
			if (song.IsVisible)
			{
				isExpanderVisible = true;
			}
		}

		anime.IsExpanded = SongVisibility.IsExpanded;
		anime.IsExpanderVisible = isExpanderVisible;
		anime.IsVisible = Search.IsVisible(anime);
	}

	private Task OpenInfoFileAsync(ObservableAnime anime)
	{
		new Process
		{
			StartInfo = new(anime.AbsoluteInfoPath)
			{
				UseShellExecute = true
			}
		}.Start();
		return Task.CompletedTask;
	}

	private async Task PasteSongAsync(ObservableAnime anime)
	{
		var clipboard = ClipboardSong!;
		anime.Songs.Add(new ObservableSong(anime, clipboard.Value));
		await _Loader.SaveAsync(anime).ConfigureAwait(true);

		if (clipboard.OnPasteCallback is not null)
		{
			await clipboard.OnPasteCallback().ConfigureAwait(true);
		}
	}

	private IObservable<Unit> ProcessSongsObservable()
	{
		return Observable.StartAsync(async token =>
		{
			var jobs = _Processor.CreateJobs(Anime);
			CurrentJob = 1;
			QueuedJobs = jobs.Count;

			// As each job progresses display its percentage
			var results = jobs.ProcessAsync(x =>
			{
				if (x.Progress.IsEnd)
				{
					++CurrentJob;
				}
				ProcessingData = x;

				TaskbarProgress.UpdateTaskbarProgress(x.Percentage);
			}, token);

			await foreach (var result in results.WithCancellation(token))
			{
				// If any result is an error stop processing and display it
				if (result.IsSuccess == false)
				{
					await CancelProcessing.Execute();
					await _MessageBoxManager.ShowNoResultAsync(new()
					{
						CanResize = true,
						Height = UIUtils.MESSAGE_BOX_HEIGHT * 5,
						Text = result.ToString(),
						Title = "Failed To Process Song",
					}).ConfigureAwait(true);
				}
			}
		})
		// On cancel/error/finish set all indicators of progress back to zero
		.Finally(() =>
		{
			ProcessingData = null;
			TaskbarProgress.UpdateTaskbarProgress(null);
		})
		// Cancel if cancel button is invoked
		.TakeUntil(CancelProcessing);
	}

	private async Task SelectDirectoryAsync()
	{
		var path = await _MessageBoxManager.GetDirectoryAsync(Directory).ConfigureAwait(true);
		if (string.IsNullOrWhiteSpace(path))
		{
			return;
		}

		Directory = path;
	}

	private Task UnloadAsync()
	{
		foreach (var subscriptions in _Subscriptions)
		{
			subscriptions.Dispose();
		}

		Anime.Clear();
		SelectedItems.Clear();
		ClipboardSong = null;
		return Task.CompletedTask;
	}
}