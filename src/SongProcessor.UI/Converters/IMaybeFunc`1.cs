namespace SongProcessor.UI.Converters;

public interface IMaybeFunc<out TRet> : IMaybeFunc
{
	TRet Use(object obj);
}