using System;

namespace AMQSongProcessor
{
	public class InvalidFileTypeException : GatheringException
	{
		public InvalidFileTypeException()
			: base(GenerateMessage(""))
		{
		}

		public InvalidFileTypeException(string path)
			: base(GenerateMessage(path), path)
		{
		}

		public InvalidFileTypeException(string path, Exception innerException)
			: base(GenerateMessage(path), path, innerException)
		{
		}

		protected InvalidFileTypeException(string message, string path)
					: base(message, path)
		{
		}

		protected InvalidFileTypeException(string message, string path, Exception innerException)
			: base(message, path, innerException)
		{
		}

		private static string GenerateMessage(string path)
			=> $"Invalid file for gathering video/audio information: {path}";
	}
}