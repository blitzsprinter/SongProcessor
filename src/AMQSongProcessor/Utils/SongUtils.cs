using System.Threading.Channels;

using AMQSongProcessor.FFmpeg;
using AMQSongProcessor.Models;
using AMQSongProcessor.Results;

namespace AMQSongProcessor.Utils
{
	public static class SongUtils
	{
		public static IEnumerable<string> GetFiles(this ISongLoader loader, string directory)
		{
			var pattern = $"*.{loader.Extension}";
			return Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories);
		}

		public static IAsyncEnumerable<IAnime> LoadFromDirectoryAsync(
			this ISongLoader loader,
			string directory,
			int? filesPerTask = null)
			=> loader.LoadFromFilesAsync(loader.GetFiles(directory), filesPerTask);

		public static IAsyncEnumerable<IAnime> LoadFromFilesAsync(
			this ISongLoader loader,
			IEnumerable<string> files,
			int? filesPerTask = null)
		{
			if (!filesPerTask.HasValue)
			{
				return loader.SlowLoadFromFilesAsync(files);
			}
			return loader.FastLoadFromFilesAsync(files, filesPerTask.Value);
		}

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
				if (result.IsSuccess is null)
				{
					return;
				}
				else if (result.IsSuccess == false)
				{
					throw new InvalidOperationException(result.ToString());
				}
			}
		}

		private static IAsyncEnumerable<IAnime> FastLoadFromFilesAsync(
			this ISongLoader loader,
			IEnumerable<string> files,
			int filesPerTask)
		{
			var channel = Channel.CreateUnbounded<IAnime>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = false,
			});

			var totalTasks = 0;
			var finishedTasks = 0;
			foreach (var chunk in files.Chunk(filesPerTask))
			{
				_ = Task.Run(async () =>
				{
					Interlocked.Increment(ref totalTasks);

					try
					{
						await foreach (var anime in loader.SlowLoadFromFilesAsync(chunk))
						{
							await channel.Writer.WriteAsync(anime).ConfigureAwait(false);
						}

						if (Interlocked.Increment(ref finishedTasks) == totalTasks)
						{
							channel.Writer.Complete();
						}
					}
					catch (Exception e)
					{
						channel.Writer.Complete(e);
					}
				});
			}

			return channel.Reader.ReadAllAsync();
		}

		private static async IAsyncEnumerable<IAnime> SlowLoadFromFilesAsync(
			this ISongLoader loader,
			IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				var anime = await loader.LoadAsync(file).ConfigureAwait(false);
				if (anime is not null)
				{
					yield return anime;
				}
			}
		}
	}
}