using System.Collections.Generic;

using AMQSongProcessor.Gatherers;
using AMQSongProcessor.UI.ViewModels;
using AMQSongProcessor.UI.Views;

using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

using Splat;

namespace AMQSongProcessor.UI
{
	public class App : Application
	{
		public override void Initialize()
			=> AvaloniaXamlLoader.Load(this);

		public override void OnFrameworkInitializationCompleted()
		{
			var window = InitializeSystemItems();
			InitializeSongItems();

			window.DataContext = InitializeSuspension();
			window.Show();
			base.OnFrameworkInitializationCompleted();
		}

		private MainWindow InitializeSystemItems()
		{
			var window = new MainWindow();
			Locator.CurrentMutable.RegisterConstant(Clipboard);
			Locator.CurrentMutable.RegisterConstant<IMessageBoxManager>(new MessageBoxManager(window));
			return window;
		}

		private void InitializeSongItems()
		{
			var gatherer = new SourceInfoGatherer
			{
				RetryUntilSuccess = true,
			};
			var loader = new SongLoader(gatherer)
			{
				RemoveIgnoredSongs = false,
			};
			Locator.CurrentMutable.RegisterConstant<ISourceInfoGatherer>(gatherer);
			Locator.CurrentMutable.RegisterConstant<ISongLoader>(loader);
			Locator.CurrentMutable.Register<ISongProcessor>(() => new SongProcessor());

			Locator.CurrentMutable.RegisterConstant<IEnumerable<IAnimeGatherer>>(new IAnimeGatherer[]
			{
				new ANNGatherer(),
				new AniDBGatherer()
			});
		}

		private MainViewModel InitializeSuspension()
		{
			var suspension = new AutoSuspendHelper(ApplicationLifetime);
			var driver = new NewtonsoftJsonSuspensionDriver("appstate.json")
			{
#if DEBUG
				DeleteOnInvalidState = false,
#endif
			};
			RxApp.SuspensionHost.CreateNewAppState = () => new MainViewModel();
			RxApp.SuspensionHost.SetupDefaultSuspendResume(driver);
			suspension.OnFrameworkInitializationCompleted();

			var state = RxApp.SuspensionHost.GetAppState<MainViewModel>();
			Locator.CurrentMutable.RegisterConstant<IScreen>(state);
			Locator.CurrentMutable.Register<IViewFor<SongViewModel>>(() => new SongView());
			Locator.CurrentMutable.Register<IViewFor<AddViewModel>>(() => new AddView());
			Locator.CurrentMutable.Register<IViewFor<EditViewModel>>(() => new EditView());
			return state;
		}
	}
}