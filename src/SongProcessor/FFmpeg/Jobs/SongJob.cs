using SongProcessor.Models;
using SongProcessor.Results;
using SongProcessor.Utils;

namespace SongProcessor.FFmpeg.Jobs
{
	public abstract class SongJob : ISongJob
	{
		public const int FFMPEG_ABORTED = -1;
		public const int FFMPEG_SUCCESS = 0;

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
			process.OnCancel((_, _) =>
			{
				// We can't just send 'q' to FFmpeg and have it quit gracefully because
				// sometimes the path never gets released and then can't get deleted
				process.Kill();
				process.WaitForExit(500);
				try
				{
					File.Delete(path);
				}
				catch { } // Nothing we can do
			}, token);
			// FFmpeg will output the information we want to std:out
			var progressBuilder = new ProgressBuilder();
			process.OutputDataReceived += (_, e) =>
			{
				if (e.Data is null)
				{
					return;
				}

				if (progressBuilder.IsNextProgressReady(e.Data, out var progress))
				{
					var data = new ProcessingData(Song.GetLength(), path, progress);
					ProcessingDataReceived?.Invoke(data);
				}
			};
			// Since we set the loglevel to error we don't need to filter
			var errors = default(List<string>);
			process.ErrorDataReceived += (_, e) =>
			{
				if (e.Data is null)
				{
					return;
				}

				errors ??= new();
				errors.Add(e.Data);
			};

			var code = await process.RunAsync(OutputMode.Async).ConfigureAwait(false);
			return code switch
			{
				FFMPEG_SUCCESS => SuccessResult.Instance,
				FFMPEG_ABORTED => CanceledResult.Instance,
				_ => new ErrorResult(code, errors ?? new()),
			};
		}

		protected abstract string GenerateArgs();

		protected virtual string GetSanitizedPath()
		{
			var path = GetUnsanitizedPath();
			var dir = Path.GetDirectoryName(path)!;
			var file = FileUtils.SanitizePath(Path.GetFileName(path));
			return Path.Combine(dir, file);
		}

		protected abstract string GetUnsanitizedPath();
	}
}