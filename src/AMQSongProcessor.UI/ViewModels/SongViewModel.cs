using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;

using AdvorangesUtils;

using AMQSongProcessor.Models;

using Avalonia.Controls;
using Avalonia.LogicalTree;

using ReactiveUI;

using Splat;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class SongViewModel : ReactiveObject, IRoutableViewModel, INavigationController
	{
		private const string LOAD = "Load";
		private const string UNLOAD = "Unload";
		private readonly IScreen? _HostScreen;
		private readonly ISongLoader _Loader;
		private readonly ISongProcessor _Processor;
		private bool _BusyProcessing;
		private int _CurrentJob;
		private string? _Directory;
		private string _DirectoryButtonText = LOAD;
		private ProcessingData? _ProcessingData;
		private int _QueuedJobs;
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
		public ReactiveCommand<Anime, Unit> ClearSource { get; }
		public ReactiveCommand<TreeView, Unit> CloseAll { get; }
		public ReactiveCommand<int, Unit> CopyANNID { get; }

		public int CurrentJob
		{
			get => _CurrentJob;
			set => this.RaiseAndSetIfChanged(ref _CurrentJob, value);
		}

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

		public ReactiveCommand<Song, Unit> EditSong { get; }
		public ReactiveCommand<TreeView, Unit> ExpandAll { get; }
		public ReactiveCommand<Unit, Unit> ExportFixes { get; }
		public IScreen HostScreen => _HostScreen ?? Locator.Current.GetService<IScreen>();

		public ReactiveCommand<Unit, Unit> Load { get; }

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

		public string UrlPathSegment => "/songs";

		public SongViewModel(IScreen? screen = null)
		{
			_HostScreen = screen;
			_Loader = Locator.Current.GetService<ISongLoader>();
			_Processor = Locator.Current.GetService<ISongProcessor>();
			_Processor.Processing = new LogProcessingToViewModel(x =>
			{
				ProcessingData = x;
				if (x.Progress.IsEnd)
				{
					++CurrentJob;
				}
			});

			CanNavigate = this
				.ObservableForProperty(x => x.BusyProcessing)
				.Select(x => !x.Value);

			var validDirectory = this
				.WhenAnyValue(x => x.Directory)
				.Select(System.IO.Directory.Exists);
			Load = ReactiveCommand.CreateFromTask(async () =>
			{
				var shouldAttemptToLoad = !Anime.Any();
				Anime.Clear();

				if (shouldAttemptToLoad)
				{
					await foreach (var anime in _Loader.LoadAsync(Directory!))
					{
						Anime.Add(anime);
					}
				}

				DirectoryButtonText = Anime.Any() ? UNLOAD : LOAD;
			}, validDirectory);

			CopyANNID = ReactiveCommand.CreateFromTask<int>(async id =>
			{
				await Avalonia.Application.Current.Clipboard.SetTextAsync(id.ToString()).CAF();
			});

			ChangeSource = ReactiveCommand.CreateFromTask<Anime>(async anime =>
			{
				var directory = anime.Directory;
				var manager = Locator.Current.GetService<IMessageBoxManager>();
				var result = await manager.GetFilesAsync(directory, "Source", false).ConfigureAwait(false);
				var path = result.SingleOrDefault();
				if (path != null)
				{
					anime.Source = Path.GetFileName(path);
					await _Loader.SaveAsync(anime).ConfigureAwait(true);
				}
			});

			ClearSource = ReactiveCommand.CreateFromTask<Anime>(async anime =>
			{
				anime.Source = null;
				await _Loader.SaveAsync(anime).ConfigureAwait(true);
			});

			AddSong = ReactiveCommand.Create<Anime>(anime =>
			{
				var song = new Song();
				anime.Songs.Add(song);

				var vm = new EditViewModel(song);
				HostScreen.Router.Navigate.Execute(vm);
			});

			EditSong = ReactiveCommand.Create<Song>(song =>
			{
				var vm = new EditViewModel(song);
				HostScreen.Router.Navigate.Execute(vm);
			});

			DeleteSong = ReactiveCommand.CreateFromTask<Song>(async song =>
			{
				var anime = song.Anime;
				var text = $"Are you sure you want to delete \"{song.Name}\" from {anime.Name}?";

				const string TITLE = "Song Deletion";
				const string YES = "Yes";
				const string NO = "No";

				var manager = Locator.Current.GetService<IMessageBoxManager>();
				var result = await manager.ShowAsync(text, TITLE, new[] { YES, NO }).ConfigureAwait(true);
				if (result == YES)
				{
					anime.Songs.Remove(song);
					await _Loader.SaveAsync(anime).ConfigureAwait(true);
				}
			});

			var hasChildren = this
				.WhenAnyValue(x => x.Anime.Count)
				.Select(x => x > 0);
			ExpandAll = ReactiveCommand.Create<TreeView>(tree =>
			{
				foreach (TreeViewItem item in tree.GetLogicalChildren())
				{
					item.IsExpanded = true;
				}
			}, hasChildren);

			CloseAll = ReactiveCommand.Create<TreeView>(tree =>
			{
				foreach (TreeViewItem item in tree.GetLogicalChildren())
				{
					item.IsExpanded = false;
				}
			}, hasChildren);

			ExportFixes = ReactiveCommand.CreateFromTask(async () =>
			{
				await _Processor.ExportFixesAsync(Directory!, Anime).ConfigureAwait(true);
			}, hasChildren);

			ProcessSongs = ReactiveCommand.CreateFromObservable(() =>
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
			}, hasChildren);

			CancelProcessing = ReactiveCommand.Create(() =>
			{
				BusyProcessing = false;
			}, ProcessSongs.IsExecuting);
		}
	}
}