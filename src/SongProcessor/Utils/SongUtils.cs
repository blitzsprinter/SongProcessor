using SongProcessor.FFmpeg;
using SongProcessor.Models;
using SongProcessor.Results;

using System.Threading.Channels;

namespace SongProcessor.Utils;

public sealed record SaveNewOptions(
	bool AddShowNameDirectory,
	bool AllowOverwrite,
	bool CreateDuplicateFile
)
{
	public static SaveNewOptions Default { get; } = new(
		AddShowNameDirectory: true,
		AllowOverwrite: false,
		CreateDuplicateFile: true
	);
}

public static class SongUtils
{
	public const int LOAD_SLOW = 0;

	public static Task ExportFixesAsync(
		this ISongProcessor processor,
		IEnumerable<IAnime> anime,
		string directory,
		string fileName = "fixes.txt",
		CancellationToken cancellationToken = default)
	{
		var path = Path.Combine(directory, fileName);
		var text = processor.ExportFixes(anime);
		return File.WriteAllTextAsync(path, text, cancellationToken);
	}

	public static IEnumerable<string> GetFiles(this ISongLoader loader, string directory)
	{
		var pattern = $"*.{loader.Extension}";
		return Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories);
	}

	public static IAsyncEnumerable<IAnime> LoadFromFilesAsync(
		this ISongLoader loader,
		IEnumerable<string> files,
		int filesPerTask = LOAD_SLOW)
	{
		if (filesPerTask < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(filesPerTask));
		}
		if (filesPerTask == LOAD_SLOW)
		{
			return loader.SlowLoadFromFilesAsync(files);
		}
		return loader.FastLoadFromFilesAsync(files, filesPerTask);
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

	public static Task SaveAsync(
		this ISongLoader loader,
		IAnime anime)
		=> loader.SaveAsync(anime.AbsoluteInfoPath, anime);

	public static async Task<string?> SaveNewAsync(
		this ISongLoader loader,
		string directory,
		IAnimeBase anime,
		SaveNewOptions options)
	{
		var dir = new DirectoryInfo(directory);
		if (options.AddShowNameDirectory)
		{
			var showDirectory = FileUtils.SanitizePath($"[{anime.Year}] {anime.Name}");
			dir = new DirectoryInfo(Path.Combine(dir.FullName, showDirectory));
		}
		dir.Create();

		var file = new FileInfo(Path.Combine(dir.FullName, $"info.{loader.Extension}"));
		if (file.Exists && options.CreateDuplicateFile)
		{
			file = new FileInfo(FileUtils.NextAvailableFile(file.FullName));
		}

		if (file.Exists && !options.AllowOverwrite)
		{
			return null;
		}

		await loader.SaveAsync(file.FullName, anime).ConfigureAwait(false);
		return file.FullName;
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