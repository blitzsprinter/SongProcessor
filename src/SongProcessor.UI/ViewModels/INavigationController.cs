namespace SongProcessor.UI.ViewModels;

public interface INavigationController
{
	IObservable<bool> CanNavigate { get; }
}