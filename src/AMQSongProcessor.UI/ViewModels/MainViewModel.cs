using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;

using AMQSongProcessor.Ffmpeg;
using AMQSongProcessor.Gatherers;

using Avalonia.Input.Platform;

using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class MainViewModel : ReactiveObject, IScreen
	{
		private RoutingState _Router = new();

		[DataMember]
		public RoutingState Router
		{
			get => _Router;
			set => this.RaiseAndSetIfChanged(ref _Router, value);
		}

		#region Commands
		public ReactiveCommand<Unit, Unit> Add { get; }
		public ReactiveCommand<Unit, Unit> GoBack { get; }
		public ReactiveCommand<Unit, Unit> Load { get; }
		#endregion Commands

		public MainViewModel(
			ISongLoader loader,
			ISongProcessor processor,
			ISourceInfoGatherer gatherer,
			IClipboard clipboard,
			IMessageBoxManager messageBoxManager,
			IEnumerable<IAnimeGatherer> gatherers)
		{
			Load = CreateViewModelCommand(() => new SongViewModel(
				this,
				loader,
				processor,
				gatherer,
				clipboard,
				messageBoxManager
			));
			Add = CreateViewModelCommand(() => new AddViewModel(
				this,
				loader,
				messageBoxManager,
				gatherers
			));
			GoBack = ReactiveCommand.Create(() =>
			{
				Router.NavigateBack.Execute();
			}, CanGoBack());
		}

		private IObservable<bool> CanGoBack()
			=> CanNavigate().CombineLatest(Router.NavigateBack.CanExecute, (x, y) => x && y);

		private IObservable<bool> CanNavigate()
		{
			return this
				.WhenAnyObservable(x => x.Router.CurrentViewModel)
				.SelectMany(x =>
				{
					if (x is INavigationController controller)
					{
						return controller.CanNavigate;
					}
					return Observable.Return(true);
				});
		}

		private IObservable<bool> CanNavigateTo<T>()
			where T : IRoutableViewModel
		{
			var isDifferent = this
				.WhenAnyObservable(x => x.Router.CurrentViewModel)
				.Select(x => !(x is T));
			return CanNavigate().CombineLatest(isDifferent, (x, y) => x && y);
		}

		private ReactiveCommand<Unit, Unit> CreateViewModelCommand<T>(Func<T> factory)
			where T : IRoutableViewModel
		{
			return ReactiveCommand.Create(() =>
			{
				Router.Navigate.Execute(factory());
			}, CanNavigateTo<T>());
		}
	}
}