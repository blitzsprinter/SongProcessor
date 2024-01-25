namespace SongProcessor.UI;

public sealed class Clipboard<T>(T value, bool isCut, Func<Task>? onPasteCallback)
{
	public bool IsCopy => !IsCut;
	public bool IsCut { get; } = isCut;
	public Func<Task>? OnPasteCallback { get; } = onPasteCallback;
	public T Value { get; } = value;
}