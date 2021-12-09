using SongProcessor.FFmpeg;
using SongProcessor.Results;

namespace SongProcessor;

public interface ISongJob
{
	event Action<ProcessingData> ProcessingDataReceived;

	Task<IResult> ProcessAsync(CancellationToken? token = null);
}
