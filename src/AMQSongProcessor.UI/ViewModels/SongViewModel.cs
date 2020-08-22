using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using AMQSongProcessor.Models;
using AMQSongProcessor.UI.Models;
using AMQSongProcessor.Utils;

using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
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
		private readonly ISourceInfoGatherer _Gatherer;
		private readonly IScreen? _HostScreen;
		private readonly ISongLoader _Loader;
		private readonly IMessageBoxManager _MessageBoxManager;
		private readonly ISongProcessor _Processor;
		private readonly IClipboard _SystemClipboard;
		private Clipboard<ISong>? _ClipboardSong;
		private int _CurrentJob;
		private string? _Directory;
		private ProcessingData? _ProcessingData;
		private int _QueuedJobs;
		private SearchTerms _Search = new SearchTerms();
		private AvaloniaList<object> _SelectedItems = new AvaloniaList<object>();
		private SongVisibility _SongVisibility = new SongVisibility();

		public ReactiveCommand<IAnime, Unit> AddSong { get; }
		public ObservableCollection<IAnime> Anime { get; }
			= new SortedObservableCollection<IAnime>(new AnimeComparer());
		public ReactiveCommand<Unit, Unit> CancelProcessing { get; }
		public IObservable<bool> CanNavigate { get; }
		public ReactiveCommand<IAnime, Unit> ChangeSource { get; }
		public ReactiveCommand<IAnime, Unit> ClearSongs { get; }
		public ReactiveCommand<IAnime, Unit> ClearSource { get; }
		public Clipboard<ISong>? ClipboardSong
		{
			get => _ClipboardSong;
			set => this.RaiseAndSetIfChanged(ref _ClipboardSong, value);
		}
		public ReactiveCommand<int, Unit> CopyANNID { get; }
		public ReactiveCommand<ISong, Unit> CopySong { get; }
		public int CurrentJob
		{
			get => _CurrentJob;
			set => this.RaiseAndSetIfChanged(ref _CurrentJob, value);
		}
		public ReactiveCommand<ISong, Unit> CutSong { get; }
		public ReactiveCommand<IAnime, Unit> DeleteAnime { get; }
		public ReactiveCommand<ISong, Unit> DeleteSong { get; }
		[DataMember]
		public string? Directory
		{
			get => _Directory;
			set => this.RaiseAndSetIfChanged(ref _Directory, value);
		}
		public ReactiveCommand<IAnime, Unit> DuplicateAnime { get; }
		public ReactiveCommand<ISong, Unit> EditSong { get; }
		public ReactiveCommand<Unit, Unit> ExportFixes { get; }
		public ReactiveCommand<IAnime, Unit> GetVolumeInfo { get; }
		public IScreen HostScreen => _HostScreen ?? Locator.Current.GetService<IScreen>();
		public IObservable<bool> IsBusy { get; }
		public ReactiveCommand<Unit, Unit> Load { get; }
		public ReactiveCommand<StatusModifier, Unit> ModifyMultipleSongsStatus { get; }
		public IObservable<bool> MultipleItemsSelected { get; }
		public IObservable<bool> OnlySongsSelected { get; }
		public ReactiveCommand<IAnime, Unit> OpenInfoFile { get; }
		public ReactiveCommand<IAnime, Unit> PasteSong { get; }
		public ProcessingData? ProcessingData
		{
			get => _ProcessingData;
			set => this.RaiseAndSetIfChanged(ref _ProcessingData, value);
		}
		public ReactiveCommand<Unit, Unit> ProcessSongs { get; }
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
		public ReactiveCommand<Unit, Unit> SelectDirectory { get; }
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
		public ReactiveCommand<Unit, Unit> Unload { get; }
		public string UrlPathSegment => "/songs";

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

			var validDirectory = this
				.WhenAnyValue(x => x.Directory)
				.Select(System.IO.Directory.Exists);
			Load = ReactiveCommand.CreateFromTask(PrivateLoad, validDirectory);
			Unload = ReactiveCommand.Create(PrivateUnload);
			CopyANNID = ReactiveCommand.CreateFromTask<int>(PrivateCopyANNID);
			OpenInfoFile = ReactiveCommand.Create<IAnime>(PrivateOpenInfoFile);
			GetVolumeInfo = ReactiveCommand.CreateFromTask<IAnime>(PrivateGetVolumeInfo);
			DuplicateAnime = ReactiveCommand.CreateFromTask<IAnime>(PrivateDuplicateAnime);
			DeleteAnime = ReactiveCommand.CreateFromTask<IAnime>(PrivateDeleteAnime);
			ClearSongs = ReactiveCommand.CreateFromTask<IAnime>(PrivateClearSongs);
			ChangeSource = ReactiveCommand.CreateFromTask<IAnime>(PrivateChangeSource);
			ClearSource = ReactiveCommand.CreateFromTask<IAnime>(PrivateClearSource);
			AddSong = ReactiveCommand.Create<IAnime>(PrivateAddSong);
			PasteSong = ReactiveCommand.CreateFromTask<IAnime>(PrivatePasteSong);
			EditSong = ReactiveCommand.Create<ISong>(PrivateEditSong);
			CopySong = ReactiveCommand.Create<ISong>(PrivateCopySong);
			CutSong = ReactiveCommand.Create<ISong>(PrivateCutSong);
			DeleteSong = ReactiveCommand.CreateFromTask<ISong>(PrivateDeleteSong);
			ExportFixes = ReactiveCommand.CreateFromTask(PrivateExportFixes);
			ProcessSongs = ReactiveCommand.CreateFromObservable(PrivateProcessSongs);
			CancelProcessing = ReactiveCommand.Create(PrivateCancelProcessing);
			SelectDirectory = ReactiveCommand.CreateFromTask(PrivateSelectDirectory);

			var loading = Load.IsExecuting;
			var processing = ProcessSongs.IsExecuting;
			IsBusy = loading.CombineLatest(processing, (x, y) => x || y);

			var loaded = this
				.WhenAnyValue(x => x.Anime.Count)
				.Select(x => x != 0);
			CanNavigate = IsBusy.CombineLatest(loaded, (x, y) => !(x || y));

			MultipleItemsSelected = this
				.WhenAnyValue(x => x.SelectedItems.Count)
				.Select(x => x > 1);

			OnlySongsSelected = this
				.WhenAnyValue(x => x.SelectedItems)
				.SelectMany(x => x.ToObservableChangeSet<AvaloniaList<object>, object>().ToCollection())
				.Select(x => x.Count == 0 || x.All(y => y is ISong));
			ModifyMultipleSongsStatus = ReactiveCommand.CreateFromTask<StatusModifier>(PrivateModifyMultipleSongsStatus);
		}

		private IAnime GetAnime(ISong search)
			=> Anime.Single(a => a.Songs.Any(s => ReferenceEquals(s, search)));

		private void PrivateAddSong(IAnime anime)
		{
			var song = new Song();
			var vm = new EditViewModel(anime, song);
			HostScreen.Router.Navigate.Execute(vm);
		}

		private void PrivateCancelProcessing()
			=> ProcessingData = null;

		private async Task PrivateChangeSource(IAnime anime)
		{
			var dir = anime.GetDirectory();
			var defFile = Path.GetFileName(anime.GetAbsoluteSourcePath());
			var result = await _MessageBoxManager.GetFilesAsync(dir, "Source", false, defFile).ConfigureAwait(true);
			if (!(result.SingleOrDefault() is string path))
			{
				return;
			}

			try
			{
				var cast = (ObservableAnime)anime;
				cast.VideoInfo = await _Gatherer.GetVideoInfoAsync(path).ConfigureAwait(true);
			}
			catch (InvalidFileTypeException)
			{
				var text = $"\"{path}\" is an invalid file for a video source.";
				await Dispatcher.UIThread.InvokeAsync(() => _MessageBoxManager.ShowAsync(text, "Invalid File")).ConfigureAwait(true);
				return;
			}

			await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);
		}

		private async Task PrivateClearSongs(IAnime anime)
		{
			var text = $"Are you sure you want to delete all songs {anime.Name}?";
			const string TITLE = "Song Clearing";

			var result = await _MessageBoxManager.ShowAsync(text, TITLE, Constants.YesNo).ConfigureAwait(true);
			if (result == Constants.YES)
			{
				anime.Songs.Clear();
				await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);
			}
		}

		private async Task PrivateClearSource(IAnime anime)
		{
			var cast = (ObservableAnime)anime;
			cast.VideoInfo = null;
			await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);
		}

		private Task PrivateCopyANNID(int id)
			=> _SystemClipboard.SetTextAsync(id.ToString());

		private void PrivateCopySong(ISong song)
		{
			var copy = new ObservableSong(song);
			ClipboardSong = new Clipboard<ISong>(copy, false, null);
		}

		private void PrivateCutSong(ISong song)
		{
			var anime = GetAnime(song);
			ClipboardSong = new Clipboard<ISong>(song, true, () =>
			{
				anime.Songs.Remove(song);
				return _Loader.SaveAsync(anime.AbsoluteInfoPath, anime);
			});
		}

		private async Task PrivateDeleteAnime(IAnime anime)
		{
			var text = $"Are you sure you want to delete {anime.Name}?";
			const string TITLE = "Anime Deletion";

			var result = await _MessageBoxManager.ShowAsync(text, TITLE, Constants.YesNo).ConfigureAwait(true);
			if (result == Constants.YES)
			{
				Anime.Remove(anime);
				File.Delete(anime.AbsoluteInfoPath);
			}
		}

		private async Task PrivateDeleteSong(ISong song)
		{
			var anime = GetAnime(song);
			var text = $"Are you sure you want to delete \"{song.Name}\" from {anime.Name}?";
			const string TITLE = "Song Deletion";

			var result = await _MessageBoxManager.ShowAsync(text, TITLE, Constants.YesNo).ConfigureAwait(true);
			if (result == Constants.YES)
			{
				anime.Songs.Remove(song);
				await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);
			}
		}

		private async Task PrivateDuplicateAnime(IAnime anime)
		{
			var file = await _Loader.SaveAsync(anime.GetDirectory(), anime, new SaveNewOptions
			{
				AllowOverwrite = false,
				CreateDuplicateFile = true,
				AddShowNameDirectory = false,
			}).ConfigureAwait(true);
			Anime.Add(new ObservableAnime(new Anime(file!, anime, anime.VideoInfo)));
		}

		private void PrivateEditSong(ISong song)
		{
			var vm = new EditViewModel(GetAnime(song), song);
			HostScreen.Router.Navigate.Execute(vm);
		}

		private Task PrivateExportFixes()
			=> _Processor.ExportFixesAsync(Directory!, Anime);

		private async Task PrivateGetVolumeInfo(IAnime anime)
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
				_ = Dispatcher.UIThread.InvokeAsync(() => _MessageBoxManager.ShowAsync(text, "Volume Info")).ConfigureAwait(true);
			}
		}

		private async Task PrivateLoad()
		{
			var files = _Loader.GetFiles(Directory!);
			await foreach (var anime in _Loader.LoadFromFilesAsync(files, 5))
			{
				// Not sure why, but without this sometimes VideoInfo is null
				if (anime.Source != null && anime.VideoInfo == null)
				{
					throw new InvalidOperationException("VideoInfo should not be null at this point.");
				}
				Anime.Add(new ObservableAnime(anime));
			}
		}

		private async Task PrivateModifyMultipleSongsStatus(StatusModifier modifier)
		{
			var isRemove = modifier < 0;
			var status = modifier.ToStatus();

			var action = isRemove ? "removing" : "adding";
			var text = $"Are you sure you want to modify {SelectedItems.Count} songs status' by {action} {status}?";
			const string TITLE = "Multiple Song Status Modification";

			var result = await _MessageBoxManager.ShowAsync(text, TITLE, Constants.YesNo).ConfigureAwait(true);
			if (result != Constants.YES)
			{
				return;
			}

			foreach (var group in SelectedItems.OfType<ISong>().GroupBy(GetAnime))
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

		private void PrivateOpenInfoFile(IAnime anime)
		{
			new Process
			{
				StartInfo = new ProcessStartInfo(anime.AbsoluteInfoPath)
				{
					UseShellExecute = true
				}
			}.Start();
		}

		private async Task PrivatePasteSong(IAnime anime)
		{
			var cp = ClipboardSong!.Value;
			anime.Songs.Add(cp.Value);
			await _Loader.SaveAsync(anime.AbsoluteInfoPath, anime).ConfigureAwait(true);

			if (cp.OnPasteCallback != null)
			{
				await cp.OnPasteCallback().ConfigureAwait(true);
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
				}, token).ConfigureAwait(true);

				ProcessingData = null;
			}).TakeUntil(CancelProcessing);
		}

		private async Task PrivateSelectDirectory()
		{
			var dir = System.IO.Directory.Exists(Directory) ? Directory! : Environment.CurrentDirectory;
			var path = await _MessageBoxManager.GetDirectoryAsync(dir, "Directory").ConfigureAwait(true);
			if (string.IsNullOrWhiteSpace(path))
			{
				return;
			}

			Directory = path;
		}

		private void PrivateUnload()
		{
			Anime.Clear();
			ClipboardSong = null;
		}
	}
}