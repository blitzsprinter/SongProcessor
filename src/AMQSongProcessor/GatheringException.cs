using System;

namespace AMQSongProcessor
{
	public abstract class GatheringException : Exception
	{
		public string? Path { get; }

		protected GatheringException()
		{
		}

		protected GatheringException(string? message)
			: base(message)
		{
		}

		protected GatheringException(string? message, string? path)
			: base(message)
		{
			Path = path;
		}

		protected GatheringException(string? message, Exception? innerException)
			: base(message, innerException)
		{
		}

		protected GatheringException(string? message, string? path, Exception? innerException)
			: base(message, innerException)
		{
			Path = path;
		}
	}
}