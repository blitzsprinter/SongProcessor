namespace SongProcessor.UI.Converters;

public interface IMaybeFunc<in TParam, out TRet> : IMaybeFunc
{
	public TRet Use(object obj, TParam param);
}
