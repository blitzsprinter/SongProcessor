using AMQSongProcessor.FFmpeg;
using AMQSongProcessor.Results;

namespace AMQSongProcessor
{
	public interface ISongJob
	{
		event Action<ProcessingData> ProcessingDataReceived;

		Task<IResult> ProcessAsync(CancellationToken? token = null);
	}
}