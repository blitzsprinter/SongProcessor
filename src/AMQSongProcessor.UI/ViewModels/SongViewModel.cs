using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

using Avalonia.Input.Platform;
using Avalonia.Threading;

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
		private Clipboard<Song>? _ClipboardSong;
		private int _CurrentJob;
		private string? _Directory;
		private ProcessingData? _ProcessingData;
		private int _QueuedJobs;
		private SearchTerms _Search = new SearchTerms();
		private SongVisibility _SongVisibility = new SongVisibility();

		public ReactiveCommand<Anime, Unit> AddSong { get; }
		public ObservableCollection<Anime> Anime { get; }
			= new SortedObservableCollection<Anime>(new AnimeComparer());
		public ReactiveCommand<Unit, Unit> CancelProcessing { get; }
		public IObservable<bool> CanNavigate { get; }
		public ReactiveCommand<Anime, Unit> ChangeSource { get; }
		public ReactiveCommand<Anime, Unit> ClearSongs { get; }
		public ReactiveCommand<Anime, Unit> ClearSource { get; }
		public Clipboard<Song>? ClipboardSong
		{
			get => _ClipboardSong;
			set => this.RaiseAndSetIfChanged(ref _ClipboardSong, value);
		}
		public ReactiveCommand<int, Unit> CopyANNID { get; }
		public ReactiveCommand<Song, Unit> CopySong { get; }
		public int CurrentJob
		{
			get => _CurrentJob;
			set => this.RaiseAndSetIfChanged(ref _CurrentJob, value);
		}
		public ReactiveCommand<Song, Unit> CutSong { get; }
		public ReactiveCommand<Anime, Unit> DeleteAnime { get; }
		public ReactiveCommand<Song, Unit> DeleteSong { get; }
		[DataMember]
		public string? Directory
		{
			get => _Directory;
			set => this.RaiseAndSetIfChanged(ref _Directory, value);
		}
		public ReactiveCommand<Anime, Unit> DuplicateAnime { get; }
		public ReactiveCommand<Song, Unit> EditSong { get; }
		public ReactiveCommand<Unit, Unit> ExportFixes { get; }
		public ReactiveCommand<Anime, Unit> GetVolumeInfo { get; }
		public IScreen HostScreen => _HostScreen ?? Locator.Current.GetService<IScreen>();
		public IObservable<bool> IsBusy { get; }
		public ReactiveCommand<Unit, Unit> Load { get; }
		public ReactiveCommand<Anime, Unit> OpenInfoFile { get; }
		public ReactiveCommand<Anime, Unit> PasteSong { get; }
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
			OpenInfoFile = ReactiveCommand.Create<Anime>(PrivateOpenInfoFile);
			GetVolumeInfo = ReactiveCommand.CreateFromTask<Anime>(PrivateGetVolumeInfo);
			DuplicateAnime = ReactiveCommand.CreateFromTask<Anime>(PrivateDuplicateAnime);
			DeleteAnime = ReactiveCommand.CreateFromTask<Anime>(PrivateDeleteAnime);
			ClearSongs = ReactiveCommand.CreateFromTask<Anime>(PrivateClearSongs);
			ChangeSource = ReactiveCommand.CreateFromTask<Anime>(PrivateChangeSource);
			ClearSource = ReactiveCommand.CreateFromTask<Anime>(PrivateClearSource);
			AddSong = ReactiveCommand.Create<Anime>(PrivateAddSong);
			PasteSong = ReactiveCommand.CreateFromTask<Anime>(PrivatePasteSong);
			EditSong = ReactiveCommand.Create<Song>(PrivateEditSong);
			CopySong = ReactiveCommand.CreateFromTask<Song>(PrivateCopySong);
			CutSong = ReactiveCommand.Create<Song>(PrivateCutSong);
			DeleteSong = ReactiveCommand.CreateFromTask<Song>(PrivateDeleteSong);
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
		}

		private void PrivateAddSong(Anime anime)
		{
			var song = new Song();
			var vm = new EditViewModel(anime, song);
			HostScreen.Router.Navigate.Execute(vm);
		}

		private void PrivateCancelProcessing()
			=> ProcessingData = null;

		private async Task PrivateChangeSource(Anime anime)
		{
			var dir = anime.Directory;
			var defFile = Path.GetFileName(anime.AbsoluteSourcePath);
			var result = await _MessageBoxManager.GetFilesAsync(dir, "Source", false, defFile).ConfigureAwait(true);
			if (!(result.SingleOrDefault() is string path))
			{
				return;
			}

			VideoInfo info;
			try
			{
				info = await _Gatherer.GetVideoInfoAsync(path).ConfigureAwait(true);
			}
			catch (InvalidFileTypeException)
			{
				var text = $"\"{path}\" is an invalid file for a video source.";
				await Dispatcher.UIThread.InvokeAsync(() => _MessageBoxManager.ShowAsync(text, "Invalid File")).ConfigureAwait(true);
				return;
			}

			anime.SetSourceFile(path, info);
			await _Loader.SaveAsync(anime).ConfigureAwait(true);
		}

		private async Task PrivateClearSongs(Anime anime)
		{
			var text = $"Are you sure you want to delete all songs {anime.Name}?";
			const string TITLE = "Song Clearing";

			var result = await _MessageBoxManager.ShowAsync(text, TITLE, Constants.YesNo).ConfigureAwait(true);
			if (result == Constants.YES)
			{
				anime.Songs.Clear();
				await _Loader.SaveAsync(anime).ConfigureAwait(true);
			}
		}

		private async Task PrivateClearSource(Anime anime)
		{
			anime.Source = null;
			await _Loader.SaveAsync(anime).ConfigureAwait(true);
			anime.VideoInfo = null;
		}

		private Task PrivateCopyANNID(int id)
			=> _SystemClipboard.SetTextAsync(id.ToString());

		private async Task PrivateCopySong(Song song)
		{
			var dupe = await _Loader.DuplicateSongAsync(song).ConfigureAwait(true);
			ClipboardSong = new Clipboard<Song>(dupe, false, null);
		}

		private void PrivateCutSong(Song song)
		{
			var anime = song.Anime;
			ClipboardSong = new Clipboard<Song>(song, true, () =>
			{
				anime.Songs.Remove(song);
				return _Loader.SaveAsync(anime);
			});
		}

		private async Task PrivateDeleteAnime(Anime anime)
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

		private async Task PrivateDeleteSong(Song song)
		{
			var anime = song.Anime;
			var text = $"Are you sure you want to delete \"{song.Name}\" from {anime.Name}?";
			const string TITLE = "Song Deletion";

			var result = await _MessageBoxManager.ShowAsync(text, TITLE, Constants.YesNo).ConfigureAwait(true);
			if (result == Constants.YES)
			{
				anime.Songs.Remove(song);
				await _Loader.SaveAsync(anime).ConfigureAwait(true);
			}
		}

		private async Task PrivateDuplicateAnime(Anime anime)
		{
			var duplicate = new Anime(anime);
			await _Loader.SaveAsync(anime, new SaveNewOptions(anime.Directory)
			{
				AllowOverwrite = false,
				CreateDuplicateFile = true,
				AddShowNameDirectory = false,
			}).ConfigureAwait(true);

			for (var i = Anime.Count - 1; i >= 0; --i)
			{
				if (Anime[i].Id != duplicate.Id)
				{
					continue;
				}

				if (i == Anime.Count - 1)
				{
					Anime.Add(duplicate);
				}
				else
				{
					Anime.Insert(i + 1, duplicate);
				}
				break;
			}
		}

		private void PrivateEditSong(Song song)
		{
			var vm = new EditViewModel(song.Anime, song);
			HostScreen.Router.Navigate.Execute(vm);
		}

		private Task PrivateExportFixes()
			=> _Processor.ExportFixesAsync(Directory!, Anime);

		private async Task PrivateGetVolumeInfo(Anime anime)
		{
			var dir = anime.Directory;
			var result = await _MessageBoxManager.GetFilesAsync(dir, "Volume Info", true).ConfigureAwait(true);
			if (result.Length == 0)
			{
				return;
			}

			foreach (var path in result)
			{
				var info = await _Gatherer.GetAverageVolumeAsync(path).ConfigureAwait(true);
				var text = $"Volume information for \"{Path.GetFileName(path)}\":" +
					$"\nMean volume: {info.MeanVolume}dB" +
					$"\nMax volume: {info.MaxVolume}dB";
				_ = Dispatcher.UIThread.InvokeAsync(() => _MessageBoxManager.ShowAsync(text, "Volume Info")).ConfigureAwait(true);
			}
		}

		private async Task PrivateLoad()
		{
			var files = _Loader.GetFiles(Directory!);
			await foreach (var anime in _Loader.LoadFromFilesAsync(files, 5))
			{
				Anime.Add(anime);
			}
		}

		private void PrivateOpenInfoFile(Anime anime)
		{
			new Process
			{
				StartInfo = new ProcessStartInfo(anime.AbsoluteInfoPath)
				{
					UseShellExecute = true
				}
			}.Start();
		}

		private async Task PrivatePasteSong(Anime anime)
		{
			var cp = ClipboardSong!.Value;
			anime.Songs.Add(cp.Value);
			await _Loader.SaveAsync(anime).ConfigureAwait(true);

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