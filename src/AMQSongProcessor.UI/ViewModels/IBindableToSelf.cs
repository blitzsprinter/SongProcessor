using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	public interface IBindableToSelf : IReactiveObject
	{
		public object Self { get; }
	}

	public interface IBindableToSelf<T> : IBindableToSelf
	{
		public new T Self { get; }
	}
}