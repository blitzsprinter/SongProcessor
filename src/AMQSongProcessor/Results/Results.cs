namespace AMQSongProcessor.Results
{
	public class FFmpegErrorResult : Result
	{
		public int Code { get; }
		public IReadOnlyList<string> Errors { get; }

		public FFmpegErrorResult(int code, IReadOnlyList<string> errors)
			: base($"FFmpeg encountered an error ({code}): {string.Join('\n', errors)}.", false)
		{
			Code = code;
			Errors = errors;
		}
	}

	public class FFmpegSuccess : Result
	{
		public static FFmpegSuccess Instance { get; } = new();

		public FFmpegSuccess() : base("FFmpeg successfully encoded the output.", true)
		{
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
}