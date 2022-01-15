namespace SongProcessor.FFmpeg;

public class SourceInfoGatheringException : Exception
{
	public string? File { get; }
	public char? Stream { get; }

	public SourceInfoGatheringException()
	{
	}

	public SourceInfoGatheringException(string? message)
		: base(message)
	{
	}

	public SourceInfoGatheringException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}

	public SourceInfoGatheringException(string file, char stream, Exception? innerException)
		: this($"Unable to gather '{stream}' stream info for {file}.", innerException)
	{
		File = file;
		Stream = stream;
	}
}