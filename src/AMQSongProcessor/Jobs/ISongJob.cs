using AMQSongProcessor.Ffmpeg;
using AMQSongProcessor.Jobs.Results;

namespace AMQSongProcessor.Jobs
{
	public interface ISongJob
	{
		event Action<ProcessingData> ProcessingDataReceived;

		Task<IResult> ProcessAsync(CancellationToken? token = null);
	}
}