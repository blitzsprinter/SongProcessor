using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using AMQSongProcessor.Ffmpeg;
using AMQSongProcessor.Models;
using AMQSongProcessor.UI.Models;
using AMQSongProcessor.Utils;

using Avalonia.Collections;
using Avalonia.Input.Platform;
using Avalonia.Threading;

using DynamicData;
using DynamicData.Binding;

using ReactiveUI;

using Splat;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class SongViewModel : ReactiveObject, IRoutableViewModel, INavigationController
	{
		private static readonly SaveNewOptions _SaveOptions = new
		(
			AddShowNameDirectory: false,
			AllowOverwrite: false,
			CreateDuplicateFile: true
		);

		private readonly ISourceInfoGatherer _Gatherer;
		private readonly IScreen? _HostScreen;
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
		public IScreen HostScreen => _HostScreen ?? Locator.Current.GetService<IScreen>();
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

		public SongViewModel() : this(null)
		{
		}

		public SongViewModel(IScreen? screen)
		{
			_HostScreen = screen;
			_Loader = Locator.Current.GetService<ISongLoader>();
			_Processor = Locator.Current.GetService<ISongProcessor>();
			_Gatherer = Locator.Current.GetService<ISourceInfoGatherer>();
			_SystemClipboard = Locator.Current.GetService<IClipboard>();
			_MessageBoxManager = Locator.Current.GetService<IMessageBoxManager>();

			Unload = ReactiveCommand.Create(PrivateUnload);
			CopyANNID = ReactiveCommand.CreateFromTask<int>(PrivateCopyANNID);
			OpenInfoFile = ReactiveCommand.Create<ObservableAnime>(PrivateOpenInfoFile);
			GetVolumeInfo = ReactiveCommand.CreateFromTask<ObservableAnime>(PrivateGetVolumeInfo);
			DuplicateAnime = ReactiveCommand.CreateFromTask<ObservableAnime>(PrivateDuplicateAnime);
			DeleteAnime = ReactiveCommand.CreateFromTask<ObservableAnime>(PrivateDeleteAnime);
			ClearSongs = ReactiveCommand.CreateFromTask<ObservableAnime>(PrivateClearSongs);
			ChangeSource = ReactiveCommand.CreateFromTask<ObservableAnime>(PrivateChangeSource);
			ClearSource = ReactiveCommand.CreateFromTask<ObservableAnime>(PrivateClearSource);
			AddSong = ReactiveCommand.Create<ObservableAnime>(PrivateAddSong);
			EditSong = ReactiveCommand.Create<ObservableSong>(PrivateEditSong);
			CopySong = ReactiveCommand.Create<ObservableSong>(PrivateCopySong);
			CutSong = ReactiveCommand.Create<ObservableSong>(PrivateCutSong);
			DeleteSong = ReactiveCommand.CreateFromTask<ObservableSong>(PrivateDeleteSong);
			ExportFixes = ReactiveCommand.CreateFromTask(PrivateExportFixes);
			ProcessSongs = ReactiveCommand.CreateFromObservable(PrivateProcessSongs);
			CancelProcessing = ReactiveCommand.Create(() => { });
			SelectDirectory = ReactiveCommand.CreateFromTask(PrivateSelectDirectory);
			ModifyMultipleSongsStatus = ReactiveCommand.CreateFromTask<StatusModifier>(PrivateModifyMultipleSongsStatus);

			var validDirectory = this
				.WhenAnyValue(x => x.Directory)
				.Select(System.IO.Directory.Exists);
			Load = ReactiveCommand.CreateFromTask(PrivateLoad, validDirectory);

			var validClipboard = this
				.WhenAnyValue(x => x.ClipboardSong)
				.Select(x => x != null);
			PasteSong = ReactiveCommand.CreateFromTask<ObservableAnime>(PrivatePasteSong, validClipboard);

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

		private void PrivateAddSong(ObservableAnime anime)
		{
			var song = new ObservableSong(anime, new Song());
			anime.Songs.Add(song);

			var vm = new EditViewModel(song);
			HostScreen.Router.Navigate.Execute(vm);
		}

		private async Task PrivateChangeSource(ObservableAnime anime)
		{
			var dir = anime.GetDirectory();
			var defFile = Path.GetFileName(anime.GetAbsoluteSourcePath());
			var result = await _MessageBoxManager.GetFilesAsync(dir, "Source", false, defFile).ConfigureAwait(true);
			if (result.SingleOrDefault() is not string path)
			{
				return;
			}

			try
			{
				anime.VideoInfo = await _Gatherer.GetVideoInfoAsync(path).ConfigureAwait(true);
			}
			catch (Exception)
			{
				await Dispatcher.UIThread.InvokeAsync(() =>
				{
					return _MessageBoxManager.ShowNoResultAsync(new()
					{
						Text = $"\"{path}\" is an invalid file for a video source.",
						Title = "Invalid File",
					});
				}).ConfigureAwait(true);
				return;
			}

			await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);
		}

		private async Task PrivateClearSongs(ObservableAnime anime)
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
			await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);
		}

		private async Task PrivateClearSource(ObservableAnime anime)
		{
			anime.VideoInfo = null;
			await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);
		}

		private Task PrivateCopyANNID(int id)
			=> _SystemClipboard.SetTextAsync(id.ToString());

		private void PrivateCopySong(ObservableSong song)
			=> ClipboardSong = new(song, false, null);

		private void PrivateCutSong(ObservableSong song)
		{
			ClipboardSong = new(song, true, () =>
			{
				var anime = song.Parent;
				anime.Songs.Remove(song);
				return _Loader.SaveAsync(anime.AbsoluteInfoPath, anime);
			});
		}

		private async Task PrivateDeleteAnime(ObservableAnime anime)
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

		private async Task PrivateDeleteSong(ObservableSong song)
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
			await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);
		}

		private async Task PrivateDuplicateAnime(ObservableAnime anime)
		{
			var file = await _Loader.SaveAsync(anime.GetDirectory(), anime, _SaveOptions).ConfigureAwait(true);
			Anime.Add(new ObservableAnime(new Anime(file!, anime, anime.VideoInfo)));
		}

		private void PrivateEditSong(ObservableSong song)
		{
			var vm = new EditViewModel(song);
			HostScreen.Router.Navigate.Execute(vm);
		}

		private Task PrivateExportFixes()
			=> _Processor.ExportFixesAsync(Directory!, Anime);

		private async Task PrivateGetVolumeInfo(ObservableAnime anime)
		{
			var dir = anime.GetDirectory();
			var paths = await _MessageBoxManager.GetFilesAsync(dir, "Volume Info", true).ConfigureAwait(true);
			if (paths.Length == 0)
			{
				return;
			}

			foreach (var path in paths)
			{
				var result = await _Gatherer.GetAverageVolumeAsync(path).ConfigureAwait(true);
				var text = $"Volume information for \"{Path.GetFileName(path)}\":" +
					$"\nMean volume: {result.Info.MeanVolume}dB" +
					$"\nMax volume: {result.Info.MaxVolume}dB";
				_ = Dispatcher.UIThread.InvokeAsync(() =>
				{
					return _MessageBoxManager.ShowNoResultAsync(new()
					{
						Text = text,
						Title = "Volume Info",
					});
				}).ConfigureAwait(true);
			}
		}

		private async Task PrivateLoad()
		{
			try
			{
				var files = _Loader.GetFiles(Directory!);
				await foreach (var anime in _Loader.LoadFromFilesAsync(files, 5))
				{
					var o = new ObservableAnime(anime);
					ModifyVisibility(o);
					Anime.Add(o);

					_Subscriptions.Add(o.Changed.Subscribe(_ => ModifyVisibility(o)));
					_Subscriptions.Add(o.Songs
						.ToObservableChangeSet()
						.WhenAnyPropertyChanged()
						.Subscribe(_ => ModifyVisibility(o))
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
				await _MessageBoxManager.ShowNoResultAsync(new()
				{
					CanResize = true,
					Height = Constants.MESSAGE_BOX_HEIGHT * 5,
					Text = e.ToString(),
					Title = "Failed To Load Songs",
				}).ConfigureAwait(true);
				throw;
			}
		}

		private async Task PrivateModifyMultipleSongsStatus(StatusModifier modifier)
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
				await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);
			}
		}

		private void PrivateOpenInfoFile(ObservableAnime anime)
		{
			new Process
			{
				StartInfo = new(anime.AbsoluteInfoPath)
				{
					UseShellExecute = true
				}
			}.Start();
		}

		private async Task PrivatePasteSong(ObservableAnime anime)
		{
			var clipboard = ClipboardSong!;
			anime.Songs.Add(new ObservableSong(anime, clipboard.Value));
			await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);

			if (clipboard.OnPasteCallback != null)
			{
				await clipboard.OnPasteCallback().ConfigureAwait(true);
			}
		}

		private IObservable<Unit> PrivateProcessSongs()
		{
			//start processing, but cancel if the cancel button is clicked
			return Observable.StartAsync(async token =>
			{
				var jobs = _Processor.CreateJobs(Anime);
				CurrentJob = 1;
				QueuedJobs = jobs.Count;

				await jobs.ProcessAsync(x =>
				{
					if (x.Progress.IsEnd)
					{
						++CurrentJob;
					}
					ProcessingData = x;

					TaskbarProgress.UpdateTaskbarProgress(x.Percentage);
				}, token).ConfigureAwait(true);
			})
			.Finally(() =>
			{
				ProcessingData = null;
				TaskbarProgress.UpdateTaskbarProgress(null);
			})
			.TakeUntil(CancelProcessing);
		}

		private async Task PrivateSelectDirectory()
		{
			var path = await _MessageBoxManager.GetDirectoryAsync(Directory).ConfigureAwait(true);
			if (string.IsNullOrWhiteSpace(path))
			{
				return;
			}

			Directory = path;
		}

		private void PrivateUnload()
		{
			foreach (var subscriptions in _Subscriptions)
			{
				subscriptions.Dispose();
			}

			Anime.Clear();
			SelectedItems.Clear();
			ClipboardSong = null;
		}
	}
}