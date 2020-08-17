using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Jobs
{
	public abstract class SongJob : ISongJob
	{
		public const int FILE_ALREADY_EXISTS = 183;
		public static readonly AspectRatio SquareSAR = new AspectRatio(1, 1);

		public event Action<ProcessingData>? ProcessingDataReceived;

		public bool AlreadyExists => File.Exists(GetValidPath());
		public Song Song { get; }
		public IAnime Anime { get; }

		protected SongJob(IAnime anime, Song song)
		{
			Anime = anime;
			Song = song;
		}

		public async Task<int> ProcessAsync(CancellationToken? token = null)
		{
			var path = GetValidPath();
			if (File.Exists(path))
			{
				return FILE_ALREADY_EXISTS;
			}

			using var process = ProcessUtils.FFmpeg.CreateProcess(GenerateArgs());
			process.WithCleanUp((s, e) =>
			{
				process.Kill();
				process.Dispose();
				//Without this sleep the file is not released in time and an exception happens
				Thread.Sleep(25);
				File.Delete(path);
			}, _ => { }, token);

			//ffmpeg will output the information we want to std:out
			var ffmpegProgressBuilder = new FfmpegProgressBuilder();
			process.OutputDataReceived += (s, e) =>
			{
				if (e.Data == null)
				{
					return;
				}

				if (ffmpegProgressBuilder.IsNextProgressReady(e.Data, out var progress))
				{
					ProcessingDataReceived?.Invoke(new ProcessingData(path, Song.Length, progress));
				}
			};

			return await process.RunAsync(false).CAF();
		}

		protected abstract string GenerateArgs();

		[Obsolete("Implement this, but use " + nameof(GetValidPath) + " instead")]
		protected abstract string GetPath();

		protected string GetValidPath()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var path = GetPath();
#pragma warning restore CS0618 // Type or member is obsolete
			var dir = Path.GetDirectoryName(path)!;
			var file = FileUtils.RemoveInvalidPathChars(Path.GetFileName(path));
			return Path.Combine(dir, file);
		}
	}
}