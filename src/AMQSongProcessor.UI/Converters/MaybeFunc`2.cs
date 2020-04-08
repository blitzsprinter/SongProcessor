using System;

namespace AMQSongProcessor.UI.Converters
{
	public sealed class MaybeFunc<TObj, TRet> : IMaybeFunc<TRet>
	{
		private readonly Func<TObj, TRet> _Func;

		public Type RequiredType => typeof(TObj);

		public MaybeFunc(Func<TObj, TRet> func)
		{
			_Func = func;
		}

		public bool CanUse(object obj)
			=> obj is TObj;

		public TRet Use(object obj)
			=> _Func((TObj)obj);
	}
}