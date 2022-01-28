namespace SongProcessor.FFmpeg;

public class ProgramException : Exception
{
	public string? Args { get; }
	public int? Code { get; }
	public Utils.Program? Program { get; }

	public ProgramException()
	{
	}

	public ProgramException(string? message) : base(message)
	{
	}

	public ProgramException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}

	public ProgramException(
		Utils.Program program,
		string args,
		int code,
		Exception? innerException = null)
		: this($"{program.Name} returned error {code} via '{args}'.", innerException)
	{
		Program = program;
		Args = args;
		Code = code;
	}
}