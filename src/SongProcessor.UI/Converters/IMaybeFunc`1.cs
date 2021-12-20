namespace SongProcessor.UI.Converters;

public interface IMaybeFunc<out TRet> : IMaybeFunc
{
	public TRet Use(object obj);
}