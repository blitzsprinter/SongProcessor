using System;

namespace AMQSongProcessor
{
	public class InvalidFileTypeException : GatheringException
	{
		public InvalidFileTypeException()
		{
		}

		public InvalidFileTypeException(string? message)
			: base(null, message)
		{
		}

		public InvalidFileTypeException(string? message, string? path)
			: base(message, path)
		{
		}

		public InvalidFileTypeException(string? message, Exception? innerException)
			: base(null, message, innerException)
		{
		}

		public InvalidFileTypeException(string? message, string? path, Exception? innerException)
			: base(message, path, innerException)
		{
		}
	}
}