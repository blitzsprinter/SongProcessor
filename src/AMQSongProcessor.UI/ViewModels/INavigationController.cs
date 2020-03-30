using System;

namespace AMQSongProcessor.UI.ViewModels
{
	public interface INavigationController
	{
		IObservable<bool> CanNavigate { get; }
	}
}