using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			string directory)
			=> loader.LoadFromFilesAsync(loader.GetFiles(directory));

		public static IAsyncEnumerable<Anime> LoadFromFilesAsync(
			this ISongLoader loader,
			IEnumerable<string> files,
			int filesPerTask)
		{
			if (filesPerTask < 1)
			{
				return loader.LoadFromFilesAsync(files);
			}
			return loader.FastLoadFromFilesAsync(files, filesPerTask);
		}

		public static async IAsyncEnumerable<Anime> LoadFromFilesAsync(
			this ISongLoader loader,
			IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				yield return await loader.LoadAsync(file).CAF();
			}
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

			var enumerators = files
				.GroupInto(filesPerTask)
				.Select(x => loader.LoadFromFilesAsync(x).GetAsyncEnumerator())
				.ToHashSet();

			try
			{
				while (enumerators.Count != 0)
				{
					var tasks = enumerators.Select(async x =>
					{
						if (await x.MoveNextAsync().CAF())
						{
							return (Enumerator: x, HasItem: true, Item: x.Current);
						}
						return (x, false, default!);
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
							enumerators.Remove(enumerator);
							await enumerator.DisposeAsync().CAF();
						}
					}
				}
			}
			finally
			{
				foreach (var enumerator in enumerators)
				{
					await enumerator.DisposeAsync().CAF();
				}
			}
		}
	}
}