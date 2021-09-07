namespace AMQSongProcessor.Results
{
	public abstract class Result : IResult
	{
		public bool IsSuccess { get; }
		public string Message { get; }

		protected Result(string message, bool isSuccess)
		{
			Message = message;
			IsSuccess = isSuccess;
		}

		public override string ToString()
			=> Message;
	}
}