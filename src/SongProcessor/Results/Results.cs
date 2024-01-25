namespace SongProcessor.Results;

public class Canceled : Result
{
	public static Canceled Instance { get; } = new();

	// IsSuccess = null because cancellation isn't really an error
	public Canceled() : base("Process was canceled.", null)
	{
	}
}

public class Error(int code, IReadOnlyList<string> errors) : Result($"Encountered an error ({code}): {string.Join('\n', errors)}.", false)
{
	public int Code { get; } = code;
	public IReadOnlyList<string> Errors { get; } = errors;
}

public class FileAlreadyExists(string file) : Result($"{file} already exists.", false)
{
	public string File { get; } = file;
}

public class Success : Result
{
	public static Success Instance { get; } = new();

	public Success() : base("Successfully encoded the output.", true)
	{
	}
}