using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;

using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class MainViewModel : ReactiveObject, IScreen
	{
		private readonly ReactiveCommand<Unit, Unit> _Add;
		private readonly ReactiveCommand<Unit, Unit> _Load;
		private RoutingState _Router = new RoutingState();

		public ICommand Add => _Add;
		public ICommand Load => _Load;

		[DataMember]
		public RoutingState Router
		{
			get => _Router;
			set => this.RaiseAndSetIfChanged(ref _Router, value);
		}

		public MainViewModel()
		{
			var canLoad = this
				.WhenAnyObservable(x => x.Router.CurrentViewModel)
				.Select(x => !(x is SongViewModel));
			_Load = ReactiveCommand.Create(() =>
			{
				Router.Navigate.Execute(new SongViewModel());
			}, canLoad);

			var canAdd = this
				.WhenAnyObservable(x => x.Router.CurrentViewModel)
				.Select(x => !(x is AddViewModel));
			_Add = ReactiveCommand.Create(() =>
			{
				Router.Navigate.Execute(new AddViewModel());
			}, canAdd);
		}
	}
}