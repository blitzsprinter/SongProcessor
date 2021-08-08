using System;
using System.Threading;
using System.Threading.Tasks;

using AMQSongProcessor.Ffmpeg;

namespace AMQSongProcessor.Jobs
{
	public interface ISongJob
	{
		event Action<ProcessingData> ProcessingDataReceived;

		Task<int> ProcessAsync(CancellationToken? token = null);
	}
}