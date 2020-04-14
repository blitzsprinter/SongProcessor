using System;

namespace AMQSongProcessor.UI.Converters
{
	public interface IMaybeFunc
	{
		public Type RequiredType { get; }

		public bool CanUse(object obj);
	}
}