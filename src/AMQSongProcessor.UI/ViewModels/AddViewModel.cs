using System;
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
	public class AddViewModel : ReactiveObject, IRoutableViewModel
	{
		private readonly IScreen? _HostScreen;
		private readonly SongLoader _Loader;
		private Anime[]? _Anime;
		private string? _Directory;
		private Exception? _Exception;
		private int _Id;
		public ICommand Add { get; }

		public Anime[]? Anime
		{
			get => _Anime;
			set => this.RaiseAndSetIfChanged(ref _Anime, value);
		}

		[DataMember]
		public string? Directory
		{
			get => _Directory;
			set => this.RaiseAndSetIfChanged(ref _Directory, value);
		}

		public Exception? Exception
		{
			get => _Exception;
			set => this.RaiseAndSetIfChanged(ref _Exception, value);
		}

		public IScreen HostScreen => _HostScreen ?? Locator.Current.GetService<IScreen>();

		[DataMember]
		public int Id
		{
			get => _Id;
			set => this.RaiseAndSetIfChanged(ref _Id, value);
		}

		public string UrlPathSegment => "/add";

		public AddViewModel(IScreen? screen = null)
		{
			_HostScreen = screen;
			_Loader = Locator.Current.GetService<SongLoader>();

			var canAdd = this.WhenAnyValue(
				x => x.Directory,
				x => x.Id,
				(directory, id) => System.IO.Directory.Exists(directory) && id > 0);
			Add = ReactiveCommand.CreateFromTask(async () =>
			{
				await Dispatcher.UIThread.InvokeAsync(async () =>
				{
					try
					{
						Anime = new[] { await _Loader.LoadFromANNAsync(Directory, Id).CAF() };
						Exception = null;
					}
					catch (Exception e)
					{
						Exception = e;
						Anime = null;
					}
				}).CAF();
			}, canAdd);
		}
	}
}