using System;

namespace AMQSongProcessor
{
	public class FileDoesNotExistException : GatheringException
	{
		public FileDoesNotExistException()
			: base(GenerateMessage(""))
		{
		}

		public FileDoesNotExistException(string path)
			: base(GenerateMessage(path), path)
		{
		}

		public FileDoesNotExistException(string path, Exception innerException)
			: base(GenerateMessage(path), path, innerException)
		{
		}

		protected FileDoesNotExistException(string message, string path)
			: base(message, path)
		{
		}

		protected FileDoesNotExistException(string message, string path, Exception innerException)
			: base(message, path, innerException)
		{
		}

		private static string GenerateMessage(string path)
			=> $"Path does not exist for gathering video/audio information: {path}";
	}
}