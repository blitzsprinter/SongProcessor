using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;

using AdvorangesUtils;

using AMQSongProcessor.Models;

using Avalonia.Threading;

using ReactiveUI;

using Splat;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class SongViewModel : ReactiveObject, IRoutableViewModel
	{
		private readonly IScreen _HostScreen;
		private readonly SongLoader _Loader;
		private string _Directory;

		public ICommand AddSong { get; }
		public ObservableCollection<Anime> Anime { get; } = new ObservableCollection<Anime>();

		public ICommand CopyANNID { get; }

		[DataMember]
		public string Directory
		{
			get => _Directory;
			set => this.RaiseAndSetIfChanged(ref _Directory, value);
		}

		public ICommand EditSong { get; }
		public IScreen HostScreen => _HostScreen ?? Locator.Current.GetService<IScreen>();
		public ICommand Load { get; }
		public ICommand RemoveSong { get; }
		public string UrlPathSegment => "/songs";

		public SongViewModel(IScreen screen = null)
		{
			_HostScreen = screen;
			_Loader = Locator.Current.GetService<SongLoader>();
			CopyANNID = ReactiveCommand.CreateFromTask<int>(async id =>
			{
				await Avalonia.Application.Current.Clipboard.SetTextAsync(id.ToString()).CAF();
			});

			var canLoad = this
				.WhenAnyValue(x => x.Directory)
				.Select(System.IO.Directory.Exists);
			Load = ReactiveCommand.CreateFromTask(async () =>
			{
				await Dispatcher.UIThread.InvokeAsync(async () =>
				{
					Anime.Clear();
					await foreach (var anime in _Loader.LoadAsync(Directory))
					{
						Anime.Add(anime);
					}
				}).CAF();
			}, canLoad);

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

			RemoveSong = ReactiveCommand.CreateFromTask<Song>(async song =>
			{
				await Dispatcher.UIThread.InvokeAsync(() =>
				{
					var anime = song.Anime;
					anime.Songs.Remove(song);
				}).CAF();
			});
		}
	}
}