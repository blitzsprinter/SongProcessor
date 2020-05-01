using System;
using System.Threading;
using System.Threading.Tasks;

namespace AMQSongProcessor.Jobs
{
	public interface ISongJob
	{
		event Action<ProcessingData> ProcessingDataReceived;

		Task<int> ProcessAsync(CancellationToken? token = null);
	}
}