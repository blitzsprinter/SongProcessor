using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
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

		private static async IAsyncEnumerable<IAnime> FastLoadFromFilesAsync(
			this ISongLoader loader,
			IEnumerable<string> files,
			int filesPerTask)
		{
			var enumerators = new ConcurrentDictionary<IAsyncEnumerator<IAnime>, bool>(files
				.GroupInto(filesPerTask)
				.Select(x => loader.SlowLoadFromFilesAsync(x).GetAsyncEnumerator())
				.ToDictionary(x => x, _ => false));
			var processed = 0;

			ValueTask DisposeEnumeratorAsync(IAsyncEnumerator<IAnime> enumerator)
			{
				try
				{
					if (enumerators.TryUpdate(enumerator, true, false))
					{
						return enumerator.DisposeAsync();
					}
				}
				catch (NotSupportedException) //When somehow diposed twice
				{
				}
				return new ValueTask();
			}

			try
			{
				while (enumerators.Count - processed > 0)
				{
					var tasks = enumerators
						.Keys
						.Select(FastLoaded<IAnime>.FromEnumerator)
						.ToHashSet();

					while (tasks.Count != 0)
					{
						var task = await Task.WhenAny(tasks).CAF();
						tasks.Remove(task);
						var fastLoaded = await task.CAF();

						if (fastLoaded.HasItem)
						{
							yield return fastLoaded.Item!;
						}
						else
						{
							Interlocked.Increment(ref processed);
							await DisposeEnumeratorAsync(fastLoaded.Enumerator).CAF();
						}
					}
				}
			}
			finally
			{
				foreach (var enumerator in enumerators.Keys)
				{
					await DisposeEnumeratorAsync(enumerator).CAF();
				}
			}
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

		private struct FastLoaded<T> where T : class
		{
			public IAsyncEnumerator<T> Enumerator { get; }
			public bool HasItem => Item != null;
			public T? Item { get; }

			public FastLoaded(IAsyncEnumerator<T> enumerator, T? item)
			{
				Enumerator = enumerator;
				Item = item;
			}

			public static async Task<FastLoaded<T>> FromEnumerator(IAsyncEnumerator<T> enumerator)
			{
				if (await enumerator.MoveNextAsync().CAF())
				{
					return new FastLoaded<T>(enumerator, enumerator.Current);
				}
				return new FastLoaded<T>(enumerator, null);
			}
		}
	}
}