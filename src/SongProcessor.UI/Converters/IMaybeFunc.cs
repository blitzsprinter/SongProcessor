namespace SongProcessor.UI.Converters;

public interface IMaybeFunc
{
	Type RequiredType { get; }

	bool CanUse(object obj);
}