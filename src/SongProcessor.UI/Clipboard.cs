namespace SongProcessor.UI
{
	public sealed class Clipboard<T>
	{
		public bool IsCopy => !IsCut;
		public bool IsCut { get; }
		public Func<Task>? OnPasteCallback { get; }
		public T Value { get; }

		public Clipboard(T value, bool isCut, Func<Task>? onPasteCallback)
		{
			Value = value;
			IsCut = isCut;
			OnPasteCallback = onPasteCallback;
		}
	}
}