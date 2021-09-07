using System.Buffers;
using System.Text.Json;

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

		public static T? ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
		{
			var bufferWriter = new ArrayBufferWriter<byte>();
			using (var writer = new Utf8JsonWriter(bufferWriter))
			{
				element.WriteTo(writer);
			}
			return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
		}

		public static T? ToObject<T>(this JsonDocument document, JsonSerializerOptions? options = null)
		{
			if (document is null)
			{
				throw new ArgumentNullException(nameof(document));
			}
			return document.RootElement.ToObject<T>(options);
		}
	}
}