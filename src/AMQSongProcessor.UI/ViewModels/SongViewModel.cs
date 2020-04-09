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

using Avalonia.Threading;

using ReactiveUI;

using Splat;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class SongViewModel : ReactiveObject, IRoutableViewModel, INavigationController
	{
		private const string LOAD = "Load";
		private const string NO = "No";
		private const string UNLOAD = "Unload";
		private const string YES = "Yes";
		private readonly ISourceInfoGatherer _Gatherer;
		private readonly IScreen? _HostScreen;
		private readonly ISongLoader _Loader;
		private readonly ISongProcessor _Processor;
		private bool _BusyProcessing;
		private Clipboard<Song>? _ClipboardSong;
		private int _CurrentJob;
		private string? _Directory;
		private string _DirectoryButtonText = LOAD;
		private ProcessingData? _ProcessingData;
		private int _QueuedJobs;
		private SearchTerms _Search = new SearchTerms();
		private SongVisibility _SongVisibility = new SongVisibility();

		public ReactiveCommand<Anime, Unit> AddSong { get; }
		public ObservableCollection<Anime> Anime { get; } = new ObservableCollection<Anime>();
		public bool BusyProcessing
		{
			get => _BusyProcessing;
			set => this.RaiseAndSetIfChanged(ref _BusyProcessing, value);
		}
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
		public string DirectoryButtonText
		{
			get => _DirectoryButtonText;
			set => this.RaiseAndSetIfChanged(ref _DirectoryButtonText, value);
		}
		public ReactiveCommand<Anime, Unit> DuplicateAnime { get; }
		public ReactiveCommand<Song, Unit> EditSong { get; }
		public ReactiveCommand<Unit, Unit> ExportFixes { get; }
		public ReactiveCommand<Anime, Unit> GetVolumeInfo { get; }
		public IScreen HostScreen => _HostScreen ?? Locator.Current.GetService<IScreen>();
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
		[DataMember]
		public SongVisibility SongVisibility
		{
			get => _SongVisibility;
			set => this.RaiseAndSetIfChanged(ref _SongVisibility, value);
		}
		public string UrlPathSegment => "/songs";

		public SongViewModel(IScreen? screen = null)
		{
			_HostScreen = screen;
			_Loader = Locator.Current.GetService<ISongLoader>();
			_Processor = Locator.Current.GetService<ISongProcessor>();
			_Gatherer = Locator.Current.GetService<ISourceInfoGatherer>();
			_Processor.Processing = new LogProcessingToViewModel(x =>
			{
				if (x.Progress.IsEnd)
				{
					++CurrentJob;
				}
				ProcessingData = x;
			});
			CanNavigate = this
				.ObservableForProperty(x => x.BusyProcessing)
				.Select(x => !x.Value);

			var validDirectory = this
				.WhenAnyValue(x => x.Directory)
				.Select(System.IO.Directory.Exists);
			Load = ReactiveCommand.CreateFromTask(PrivateLoad, validDirectory);
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
		}

		private void PrivateAddSong(Anime anime)
		{
			var song = new Song();
			var vm = new EditViewModel(anime, song);
			HostScreen.Router.Navigate.Execute(vm);
		}

		private void PrivateCancelProcessing()
		{
			BusyProcessing = false;
			ProcessingData = null;
		}

		private async Task PrivateChangeSource(Anime anime)
		{
			var dir = anime.Directory;
			var defFile = Path.GetFileName(anime.AbsoluteSourcePath);
			var manager = Locator.Current.GetService<IMessageBoxManager>();
			var result = await manager.GetFilesAsync(dir, "Source", false, defFile).ConfigureAwait(false);
			if (!(result.SingleOrDefault() is string filePath))
			{
				return;
			}

			try
			{
				anime.VideoInfo = await _Gatherer.GetVideoInfoAsync(filePath).ConfigureAwait(true);
			}
			catch (GatheringException)
			{
				var text = $"\"{filePath}\" is an invalid file for a video source.";
				await Dispatcher.UIThread.InvokeAsync(() => manager.ShowAsync(text, "Invalid File")).ConfigureAwait(true);
				return;
			}

			anime.SetSourceFile(filePath);
			await _Loader.SaveAsync(anime).ConfigureAwait(true);
		}

		private async Task PrivateClearSongs(Anime anime)
		{
			var text = $"Are you sure you want to delete all songs {anime.Name}?";
			const string TITLE = "Song Clearing";

			var manager = Locator.Current.GetService<IMessageBoxManager>();
			var result = await manager.ShowAsync(text, TITLE, new[] { YES, NO }).ConfigureAwait(true);
			if (result == YES)
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
			=> Avalonia.Application.Current.Clipboard.SetTextAsync(id.ToString());

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

			var manager = Locator.Current.GetService<IMessageBoxManager>();
			var result = await manager.ShowAsync(text, TITLE, new[] { YES, NO }).ConfigureAwait(true);
			if (result == YES)
			{
				var item = Anime.First(x => x.Id == anime.Id);
				Anime.Remove(item);
				File.Delete(anime.AbsoluteInfoPath);
			}
		}

		private async Task PrivateDeleteSong(Song song)
		{
			var anime = song.Anime;
			var text = $"Are you sure you want to delete \"{song.Name}\" from {anime.Name}?";
			const string TITLE = "Song Deletion";

			var manager = Locator.Current.GetService<IMessageBoxManager>();
			var result = await manager.ShowAsync(text, TITLE, new[] { YES, NO }).ConfigureAwait(true);
			if (result == YES)
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
			var manager = Locator.Current.GetService<IMessageBoxManager>();
			var result = await manager.GetFilesAsync(dir, "Volume Info", false).ConfigureAwait(false);
			if (!(result.SingleOrDefault() is string filePath))
			{
				return;
			}

			var info = await _Gatherer.GetAverageVolumeAsync(filePath).ConfigureAwait(true);
			var text = $"Volume information for \"{Path.GetFileName(filePath)}\":" +
				$"\nMean volume: {info.MeanVolume}dB" +
				$"\nMax volume: {info.MaxVolume}dB";
			await Dispatcher.UIThread.InvokeAsync(() => manager.ShowAsync(text, "Volume Info")).ConfigureAwait(true);
		}

		private async Task PrivateLoad()
		{
			var shouldAttemptToLoad = !Anime.Any();
			Anime.Clear();

			if (shouldAttemptToLoad)
			{
				ClipboardSong = null;
				await foreach (var anime in _Loader.LoadFromDirectoryAsync(Directory!))
				{
					Anime.Add(anime);
				}
			}

			DirectoryButtonText = Anime.Any() ? UNLOAD : LOAD;
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
				BusyProcessing = true;

				var jobs = _Processor.CreateJobs(Anime);
				CurrentJob = 1;
				QueuedJobs = jobs.Count;

				await _Processor.ProcessAsync(jobs, token).ConfigureAwait(true);

				BusyProcessing = false;
				ProcessingData = null;
			}).TakeUntil(CancelProcessing);
		}
	}
}