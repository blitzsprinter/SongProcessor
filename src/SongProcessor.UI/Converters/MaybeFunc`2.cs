namespace SongProcessor.UI.Converters
{
	public class MaybeFunc<TObj, TRet> : MaybeFunc<TObj>, IMaybeFunc<TRet>
	{
		private readonly Func<TObj, TRet> _Func;

		public MaybeFunc(Func<TObj, TRet> func)
		{
			_Func = func;
		}

		public TRet Use(object obj)
			=> _Func((TObj)obj);
	}

	public class MaybeFuncCollectionBuilder<TRet>
	{
		private readonly List<IMaybeFunc<TRet>> _Funcs = new();

		public MaybeFuncCollectionBuilder<TRet> Add<TObj>(Func<TObj, TRet> func)
		{
			_Funcs.Add(new MaybeFunc<TObj, TRet>(func));
			return this;
		}

		public IMaybeFunc<TRet>[] ToArray()
			=> _Funcs.ToArray();
	}
}