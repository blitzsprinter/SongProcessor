namespace SongProcessor.Results;

public class Canceled : Result
{
	public static Canceled Instance { get; } = new();

	// IsSuccess = null because cancellation isn't really an error
	public Canceled() : base("Process was canceled.", null)
	{
	}
}

public class Error : Result
{
	public int Code { get; }
	public IReadOnlyList<string> Errors { get; }

	public Error(int code, IReadOnlyList<string> errors)
		: base($"Encountered an error ({code}): {string.Join('\n', errors)}.", false)
	{
		Code = code;
		Errors = errors;
	}
}

public class FileAlreadyExists : Result
{
	public string File { get; }

	public FileAlreadyExists(string file) : base($"{file} already exists.", false)
	{
		File = file;
	}
}

public class Success : Result
{
	public static Success Instance { get; } = new();

	public Success() : base("Successfully encoded the output.", true)
	{
	}
}