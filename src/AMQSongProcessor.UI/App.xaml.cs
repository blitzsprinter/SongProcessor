using System.Collections.Generic;
using System.Net.Http;
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
			InitializeSongItems();
			InitializeGatherers();
			InitializeSuspension();

			var state = RxApp.SuspensionHost.GetAppState<MainViewModel>();
			Locator.CurrentMutable.RegisterConstant<IScreen>(state);
			Locator.CurrentMutable.Register<IViewFor<SongViewModel>>(() => new SongView());
			Locator.CurrentMutable.Register<IViewFor<AddViewModel>>(() => new AddView());
			Locator.CurrentMutable.Register<IViewFor<EditViewModel>>(() => new EditView());

			var window = new MainWindow
			{
				DataContext = state
			};
			Locator.CurrentMutable.RegisterConstant<IMessageBoxManager>(new MessageBoxManager(window));

			window.Show();
			base.OnFrameworkInitializationCompleted();
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
		}

		private void InitializeGatherers()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
			client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, default, br");
			client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9"); //Make sure we get English results
			client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
			client.DefaultRequestHeaders.Add("Connection", "keep-alive");
			client.DefaultRequestHeaders.Add("pragma", "no-cache");
			client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36");

			Locator.CurrentMutable.RegisterConstant(client);
			Locator.CurrentMutable.RegisterConstant<IEnumerable<IAnimeGatherer>>(new IAnimeGatherer[]
			{
				new ANNGatherer(client),
				new AniDBGatherer(client)
			});
		}

		private void InitializeSuspension()
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
		}
	}
}