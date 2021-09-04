namespace AMQSongProcessor.Ffmpeg
{
	public class InvalidFileTypeException : SourceInfoGatheringException
	{
		public InvalidFileTypeException()
		{
		}

		public InvalidFileTypeException(string message) : base(message)
		{
		}

		public InvalidFileTypeException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}