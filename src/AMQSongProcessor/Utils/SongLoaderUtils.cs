using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Utils
{
	public static class SongLoaderUtils
	{
		public static IEnumerable<string> GetFiles(this ISongLoader loader, string directory)
		{
			var pattern = "*." + loader.Extension;
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

			var totalCount = 0;
			var currentCount = 0;
			foreach (var chunk in files.Chunk(filesPerTask))
			{
				_ = Task.Run(async () =>
				{
					Interlocked.Increment(ref totalCount);

					try
					{
						await foreach (var anime in loader.SlowLoadFromFilesAsync(chunk))
						{
							await channel.Writer.WriteAsync(anime).ConfigureAwait(false);
						}

						if (Interlocked.Increment(ref currentCount) == totalCount)
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
				var anime = await loader.LoadAsync(file).CAF();
				if (anime != null)
				{
					yield return anime;
				}
			}
		}
	}
}