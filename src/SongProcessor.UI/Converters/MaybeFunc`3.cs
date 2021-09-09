namespace SongProcessor.UI.Converters
{
	public class MaybeFunc<TObj, TParam, TRet> : MaybeFunc<TObj>, IMaybeFunc<TParam, TRet>
	{
		private readonly Func<TObj, TParam, TRet> _Func;

		public MaybeFunc(Func<TObj, TParam, TRet> func)
		{
			_Func = func;
		}

		public TRet Use(object obj, TParam param)
			=> _Func((TObj)obj, param);
	}

	public class MaybeFuncCollectionBuilder<TParam, TRet>
	{
		private readonly List<IMaybeFunc<TParam, TRet>> _Funcs = new();

		public MaybeFuncCollectionBuilder<TParam, TRet> Add<TObj>(Func<TObj, TParam, TRet> func)
		{
			_Funcs.Add(new MaybeFunc<TObj, TParam, TRet>(func));
			return this;
		}

		public IMaybeFunc<TParam, TRet>[] ToArray()
			=> _Funcs.ToArray();
	}
}