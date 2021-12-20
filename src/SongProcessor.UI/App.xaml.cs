using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

using SongProcessor.FFmpeg;
using SongProcessor.Gatherers;
using SongProcessor.UI.ViewModels;
using SongProcessor.UI.Views;

using Splat;

namespace SongProcessor.UI;

public class App : Application
{
	public override void Initialize()
		=> AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
		{
			var path = Path.Combine(Directory.GetCurrentDirectory(), "CrashLog.txt");
			var text = $"[{DateTime.UtcNow:G}] {e.ExceptionObject}\n";
			File.AppendAllText(path, text);
		};

		var window = new MainWindow();
		var messageBoxManager = new MessageBoxManager(window);
		// Create a wrapper for the not yet created state
		// so when deserializing the saved view models IScreen isn't null
		var screenWrapper = new HostScreenWrapper();
		Locator.CurrentMutable.RegisterConstant(Clipboard);
		Locator.CurrentMutable.RegisterConstant<IMessageBoxManager>(messageBoxManager);
		Locator.CurrentMutable.RegisterConstant<IScreen>(screenWrapper);

		var gatherer = new SourceInfoGatherer();
		var loader = new SongLoader(gatherer);
		var processor = new SongProcessor();
		var gatherers = new IAnimeGatherer[]
		{
				new ANNGatherer(),
				new AniDBGatherer()
		};
		Locator.CurrentMutable.RegisterConstant<ISourceInfoGatherer>(gatherer);
		Locator.CurrentMutable.RegisterConstant<ISongLoader>(loader);
		Locator.CurrentMutable.RegisterConstant<ISongProcessor>(processor);
		Locator.CurrentMutable.RegisterConstant<IEnumerable<IAnimeGatherer>>(gatherers);

		// Set up suspension to save view model information
		var suspension = new AutoSuspendHelper(ApplicationLifetime);
		var driver = new NewtonsoftJsonSuspensionDriver("appstate.json")
		{
#if DEBUG
			DeleteOnInvalidState = false,
#endif
		};
		RxApp.SuspensionHost.CreateNewAppState = () =>
		{
			return new MainViewModel(
				loader,
				processor,
				gatherer,
				Clipboard,
				messageBoxManager,
				gatherers
			);
		};
		RxApp.SuspensionHost.SetupDefaultSuspendResume(driver);
		suspension.OnFrameworkInitializationCompleted();

		var state = screenWrapper.Screen = RxApp.SuspensionHost.GetAppState<MainViewModel>();
		Locator.CurrentMutable.Register<IViewFor<SongViewModel>>(() => new SongView());
		Locator.CurrentMutable.Register<IViewFor<AddViewModel>>(() => new AddView());
		Locator.CurrentMutable.Register<IViewFor<EditViewModel>>(() => new EditView());

		window.DataContext = state;
		window.Show();
		base.OnFrameworkInitializationCompleted();
	}

	private class HostScreenWrapper : IScreen
	{
		internal IScreen Screen { get; set; } = null!;

		RoutingState IScreen.Router => Screen.Router;
	}
}