using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Jobs
{
	public abstract class SongJob : ISongJob
	{
		public const int FILE_ALREADY_EXISTS = 183;
		public static readonly AspectRatio SquareSAR = new AspectRatio(1, 1);

		public bool AlreadyExists => File.Exists(GetValidPath());
		public IProgress<ProcessingData>? Processing { get; set; }
		public Song Song { get; }

		protected SongJob(Song song)
		{
			Song = song;
		}

		public async Task<int> ProcessAsync(CancellationToken? token = null)
		{
			var path = GetValidPath();
			if (File.Exists(path))
			{
				return FILE_ALREADY_EXISTS;
			}

			var args = GenerateArgs();
			using var process = Utils.CreateProcess(Utils.FFmpeg, args);

			//ffmpeg will output the information we want to std:out
			var ffmpegProgress = new MutableFfmpegProgress();
			process.OutputDataReceived += (s, e) =>
			{
				if (e.Data == null)
				{
					return;
				}

				if (ffmpegProgress.IsNextProgressReady(e.Data, out var progress))
				{
					Processing?.Report(new ProcessingData(path, Song.Length, progress));
				}
			};

			process.WithCleanUp((s, e) =>
			{
				process.Kill();
				process.Dispose();
				//Without this sleep the file is not released in time and an exception happens
				Thread.Sleep(25);
				File.Delete(path);
			}, _ => { }, token);

			return await process.RunAsync(false).CAF();
		}

		protected abstract string GenerateArgs();

		[Obsolete("Use " + nameof(GetValidPath) + " instead")]
		protected abstract string GetPath();

		protected string GetValidPath()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var path = GetPath();
#pragma warning restore CS0618 // Type or member is obsolete
			var dir = Path.GetDirectoryName(path)!;
			var file = Utils.RemoveInvalidPathChars(Path.GetFileName(path));
			return Path.Combine(dir, file);
		}
	}
}