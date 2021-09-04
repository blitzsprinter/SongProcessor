using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using AdvorangesUtils;

using AMQSongProcessor.Ffmpeg;
using AMQSongProcessor.Jobs;
using AMQSongProcessor.Jobs.Results;

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
					yield return await job.ProcessAsync(token).CAF();
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

		public static T[] ToArray<T>(this IEnumerable<T> source, int count)
		{
			if (source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			var array = new T[count];
			var i = 0;
			foreach (var item in source)
			{
				array[i++] = item;
			}
			return array;
		}

		public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable)
		{
			var list = new List<T>();
			await foreach (var value in enumerable)
			{
				list.Add(value);
			}
			return list;
		}

		[return: MaybeNull]
		public static T ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
		{
			var bufferWriter = new ArrayBufferWriter<byte>();
			using (var writer = new Utf8JsonWriter(bufferWriter))
			{
				element.WriteTo(writer);
			}
			return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
		}

		[return: MaybeNull]
		public static T ToObject<T>(this JsonDocument document, JsonSerializerOptions? options = null)
		{
			if (document is null)
			{
				throw new ArgumentNullException(nameof(document));
			}
			return document.RootElement.ToObject<T>(options);
		}
	}
}