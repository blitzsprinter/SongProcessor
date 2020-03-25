using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;

using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class MainViewModel : ReactiveObject, IScreen
	{
		private RoutingState _Router = new RoutingState();

		public ICommand Add { get; }
		public ICommand Load { get; }

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
			Load = ReactiveCommand.Create(() =>
			{
				Router.Navigate.Execute(new SongViewModel());
			}, canLoad);

			var canAdd = this
				.WhenAnyObservable(x => x.Router.CurrentViewModel)
				.Select(x => !(x is AddViewModel));
			Add = ReactiveCommand.Create(() =>
			{
				Router.Navigate.Execute(new AddViewModel());
			}, canAdd);
		}
	}
}