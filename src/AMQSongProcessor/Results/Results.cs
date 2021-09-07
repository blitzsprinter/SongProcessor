namespace AMQSongProcessor.Results
{
	public class CanceledResult : Result
	{
		public static CanceledResult Instance { get; } = new();

		public CanceledResult() : base("Process was canceled.", false)
		{
		}
	}

	public class ErrorResult : Result
	{
		public int Code { get; }
		public IReadOnlyList<string> Errors { get; }

		public ErrorResult(int code, IReadOnlyList<string> errors)
			: base($"Encountered an error ({code}): {string.Join('\n', errors)}.", false)
		{
			Code = code;
			Errors = errors;
		}
	}

	public class FileAlreadyExistsResult : Result
	{
		public string Path { get; }

		public FileAlreadyExistsResult(string path)
			: base($"{path} already exists.", false)
		{
			Path = path;
		}
	}

	public class SuccessResult : Result
	{
		public static SuccessResult Instance { get; } = new();

		public SuccessResult() : base("Successfully encoded the output.", true)
		{
		}
	}
}