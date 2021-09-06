using System.Diagnostics;

using AMQSongProcessor.Ffmpeg;
using AMQSongProcessor.Jobs.Results;
using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Jobs
{
	public abstract class SongJob : ISongJob
	{
		public const int FFMPEG_SUCCESS = 0;
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

		public async Task<IResult> ProcessAsync(CancellationToken? token = null)
		{
			var path = GetSanitizedPath();
			if (File.Exists(path))
			{
				return new FileAlreadyExistsResult(path);
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
			process.OutputDataReceived += (_, e) =>
			{
				if (e.Data is null)
				{
					return;
				}

				if (ffmpegProgressBuilder.IsNextProgressReady(e.Data, out var progress))
				{
					var data = new ProcessingData(Song.GetLength(), path, progress);
					ProcessingDataReceived?.Invoke(data);
				}
			};
			var ffmpegErrors = default(List<string>);
			process.ErrorDataReceived += (_, e) =>
			{
				if (e.Data is null)
				{
					return;
				}

				Debug.WriteLine(e.Data);

				ffmpegErrors ??= new();
				ffmpegErrors.Add(e.Data);
			};

			var code = await process.RunAsync(OutputMode.Async).ConfigureAwait(false);
			if (code != FFMPEG_SUCCESS)
			{
				ffmpegErrors ??= new();
				return new FFmpegErrorResult(code, ffmpegErrors);
			}
			return FFmpegSuccess.Instance;
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