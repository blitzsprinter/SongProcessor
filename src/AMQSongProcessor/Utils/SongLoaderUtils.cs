using System.Collections.Concurrent;
using System.Collections.Generic;
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
			var pattern = $"*.{loader.Extension}";
			return Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories);
		}

		public static IAsyncEnumerable<Anime> LoadFromDirectoryAsync(
			this ISongLoader loader,
			string directory,
			int? filesPerTask = null)
			=> loader.LoadFromFilesAsync(loader.GetFiles(directory), filesPerTask);

		public static IAsyncEnumerable<Anime> LoadFromFilesAsync(
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

		private static async IAsyncEnumerable<Anime> FastLoadFromFilesAsync(
			this ISongLoader loader,
			IEnumerable<string> files,
			int filesPerTask)
		{
			/*
			var tasks = files
				.GroupInto(filesPerTask)
				.Select(x => loader.LoadFromFilesAsync(x).ToListAsync())
				.ToList();

			while (tasks.Count != 0)
			{
				var task = await Task.WhenAny(tasks).CAF();
				tasks.Remove(task);
				foreach (var value in await task.CAF())
				{
					yield return value;
				}
			}*/

			var enumerators = new ConcurrentDictionary<IAsyncEnumerator<Anime>, bool>(files
				.GroupInto(filesPerTask)
				.Select(x => loader.SlowLoadFromFilesAsync(x).GetAsyncEnumerator())
				.ToDictionary(x => x, _ => false));
			var processed = 0;

			try
			{
				while (enumerators.Count - processed > 0)
				{
					var tasks = enumerators.Select(async kvp =>
					{
						var i = kvp.Key;
						if (await i.MoveNextAsync().CAF())
						{
							return (Enumerator: i, HasItem: true, Item: i.Current);
						}
						return (i, false, default!);
					}).ToList();

					while (tasks.Count != 0)
					{
						var task = await Task.WhenAny(tasks).CAF();

						tasks.Remove(task);
						var (enumerator, hasItem, item) = await task.CAF();

						if (hasItem)
						{
							yield return item;
						}
						else
						{
							Interlocked.Increment(ref processed);
							if (enumerators.TryUpdate(enumerator, true, false))
							{
								await enumerator.DisposeAsync().CAF();
							}
						}
					}
				}
			}
			finally
			{
				foreach (var enumerator in enumerators.Keys)
				{
					if (enumerators.TryUpdate(enumerator, true, false))
					{
						await enumerator.DisposeAsync().CAF();
					}
				}
			}
		}

		private static async IAsyncEnumerable<Anime> SlowLoadFromFilesAsync(
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