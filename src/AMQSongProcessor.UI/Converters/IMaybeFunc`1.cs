using System;

namespace AMQSongProcessor.UI.Converters
{
	public interface IMaybeFunc<out TRet>
	{
		public Type RequiredType { get; }

		public bool CanUse(object obj);

		public TRet Use(object obj);
	}
}