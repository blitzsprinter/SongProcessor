namespace SongProcessor.UI.Converters
{
	public abstract class MaybeFunc<TObj> : IMaybeFunc
	{
		public Type RequiredType => typeof(TObj);

		public bool CanUse(object obj)
			=> obj is TObj;
	}
}