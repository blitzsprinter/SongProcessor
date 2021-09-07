using AMQSongProcessor.FFmpeg;
using AMQSongProcessor.Results;

namespace AMQSongProcessor.Utils
{
	public static class Utils
	{
		public static async IAsyncEnumerable<IResult> ProcessAsync(
			this IEnumerable<ISongJob> jobs,
			Action<ProcessingData>? onProcessingDataReceived = null,
			CancellationToken? token = null)
		{
			foreach (var job in jobs)
			{
				token?.ThrowIfCancellationRequested();
				job.ProcessingDataReceived += onProcessingDataReceived;

				try
				{
					yield return await job.ProcessAsync(token).ConfigureAwait(false);
				}
				finally
				{
					job.ProcessingDataReceived -= onProcessingDataReceived;
				}
			}
		}

		public static async Task ThrowIfAnyErrors(this IAsyncEnumerable<IResult> results)
		{
			await foreach (var result in results)
			{
				if (!result.IsSuccess)
				{
					throw new InvalidOperationException(result.ToString());
				}
			}
		}
	}
}