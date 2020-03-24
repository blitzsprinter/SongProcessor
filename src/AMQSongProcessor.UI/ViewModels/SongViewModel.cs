using System.Collections.ObjectModel;
using System.Reactive;
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
		private string _Directory;

		public ObservableCollection<Anime> Anime { get; } = new ObservableCollection<Anime>();

		public ICommand CopyANNID { get; }

		[DataMember]
		public string Directory
		{
			get => _Directory;
			set => this.RaiseAndSetIfChanged(ref _Directory, value);
		}

		public IScreen HostScreen { get; }
		public ICommand Load { get; }
		public string UrlPathSegment => "/songs";

		public SongViewModel(IScreen screen = null)
		{
			HostScreen = screen ?? Locator.Current.GetService<IScreen>();

			var loader = new SongLoader
			{
				RemoveIgnoredSongs = false,
			};

			var canLoad = this
				.WhenAnyValue(x => x.Directory)
				.Select(System.IO.Directory.Exists);
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
			}, canLoad);

			CopyANNID = ReactiveCommand.CreateFromTask<int>(async id =>
			{
				await Avalonia.Application.Current.Clipboard.SetTextAsync(id.ToString()).CAF();
			});
		}
	}
}