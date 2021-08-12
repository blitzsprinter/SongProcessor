using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Ffmpeg;
using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Jobs
{
	public abstract class SongJob : ISongJob
	{
		public const int FILE_ALREADY_EXISTS = 183;
		public static readonly AspectRatio SquareSAR = new(1, 1);

		public bool AlreadyExists => File.Exists(GetSanitizedPath());
		public IAnime Anime { get; }
		public ISong Song { get; }

		public event Action<ProcessingData>? ProcessingDataReceived;

		protected SongJob(IAnime anime, ISong song)
		{
			Anime = anime;
			Song = song;
		}

		public async Task<int> ProcessAsync(CancellationToken? token = null)
		{
			var path = GetSanitizedPath();
			if (File.Exists(path))
			{
				return FILE_ALREADY_EXISTS;
			}

			using var process = ProcessUtils.FFmpeg.CreateProcess(GenerateArgs());
			process.WithCleanUp((s, e) =>
			{
				process.Kill();
				process.Dispose();
				// Without this sleep the file is not released in time and an exception happens
				Thread.Sleep(25);
				File.Delete(path);
			}, null, token);

			// ffmpeg will output the information we want to std:out
			var ffmpegProgressBuilder = new FfmpegProgressBuilder();
			process.OutputDataReceived += (s, e) =>
			{
				if (e.Data is null)
				{
					return;
				}

				if (ffmpegProgressBuilder.IsNextProgressReady(e.Data, out var progress))
				{
					ProcessingDataReceived?.Invoke(new ProcessingData(path, Song.GetLength(), progress));
				}
			};

			return await process.RunAsync(OutputMode.Async).CAF();
		}

		protected abstract string GenerateArgs();

		protected string GetSanitizedPath()
		{
			var path = GetUnsanitizedPath();
			var dir = Path.GetDirectoryName(path)!;
			var file = FileUtils.SanitizePath(Path.GetFileName(path));
			return Path.Combine(dir, file);
		}

		protected abstract string GetUnsanitizedPath();
	}
}