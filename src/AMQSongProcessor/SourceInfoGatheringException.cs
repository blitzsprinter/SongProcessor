using System;

namespace AMQSongProcessor
{
	public class SourceInfoGatheringException : Exception
	{
		public SourceInfoGatheringException()
		{
		}

		public SourceInfoGatheringException(string message) : base(message)
		{
		}

		public SourceInfoGatheringException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}