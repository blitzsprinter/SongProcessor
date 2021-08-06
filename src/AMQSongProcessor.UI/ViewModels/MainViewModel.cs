using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;

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

		public MainViewModel()
		{
			Load = CreateViewModelCommand<SongViewModel>();
			Add = CreateViewModelCommand<AddViewModel>();
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

		private ReactiveCommand<Unit, Unit> CreateViewModelCommand<T>()
			where T : IRoutableViewModel, new()
		{
			return ReactiveCommand.Create(() =>
			{
				Router.Navigate.Execute(new T());
			}, CanNavigateTo<T>());
		}
	}
}