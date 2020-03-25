using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Input;

using AdvorangesUtils;

using AMQSongProcessor.Models;

using Avalonia.Threading;

using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

using Splat;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class SongViewModel : ReactiveObject, IRoutableViewModel, IValidatableViewModel
	{
		private readonly IScreen _HostScreen;
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
		public ValidationContext ValidationContext { get; } = new ValidationContext();

		public SongViewModel(IScreen screen = null)
		{
			_HostScreen = screen;

			var loader = new SongLoader
			{
				RemoveIgnoredSongs = false,
			};

			CopyANNID = ReactiveCommand.CreateFromTask<int>(async id =>
			{
				await Avalonia.Application.Current.Clipboard.SetTextAsync(id.ToString()).CAF();
			});

			this.ValidationRule(
				x => x.Directory,
				System.IO.Directory.Exists,
				"Directory must exist.");
			Load = ReactiveCommand.CreateFromTask(async () =>
			{
				await Dispatcher.UIThread.InvokeAsync(async () =>
				{
					Anime.Clear();
					await foreach (var anime in loader.LoadAsync(Directory))
					{
						Anime.Add(anime);
					}
				}).CAF();
			}, this.IsValid());

			AddSong = ReactiveCommand.Create<Anime>(anime =>
			{
				var song = new Song();
				anime.Songs.Add(song);

				HostScreen.Router.Navigate.Execute(new EditViewModel(song));
			});

			EditSong = ReactiveCommand.Create<Song>(song =>
			{
				HostScreen.Router.Navigate.Execute(new EditViewModel(song));
			});

			RemoveSong = ReactiveCommand.Create<Song>(song =>
			{
				song.Anime.Songs.Remove(song);
			});
		}
	}
}