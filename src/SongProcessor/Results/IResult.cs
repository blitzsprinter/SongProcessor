namespace SongProcessor.Results
{
	public interface IResult
	{
		bool? IsSuccess { get; }
		string Message { get; }
	}
}