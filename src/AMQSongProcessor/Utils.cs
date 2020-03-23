using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using AdvorangesUtils;

namespace AMQSongProcessor
{
	public static class Utils
	{
		private static readonly bool IsWindows = Environment.OSVersion.Platform.ToString().CaseInsContains("win");
		public static string FFmpeg { get; } = FindProgram("ffmpeg");
		public static string FFprobe { get; } = FindProgram("ffprobe");

		public static Process CreateProcess(string program, string args)
		{
			return new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = program,
					Arguments = args,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				},
			};
		}

		public static Task<int> RunAsync(this Process process, bool write)
		{
			void HandleClosing(object source, EventArgs e)
			{
				process.Kill();
				process.Dispose();
			}

			var tcs = new TaskCompletionSource<int>();

			process.EnableRaisingEvents = true;

			//If the program gets shut down, make sure it also shuts down the ffmpeg process
			AppDomain.CurrentDomain.ProcessExit += HandleClosing;
			process.Exited += (s, e) =>
			{
				AppDomain.CurrentDomain.ProcessExit -= HandleClosing;
				tcs.SetResult(process.ExitCode);
			};

			if (write)
			{
				process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
				process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);
			}

			var started = process.Start();
			if (!started)
			{
				throw new InvalidOperationException("Could not start process: " + process);
			}

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			return tcs.Task;
		}

		public static T[] ToArray<T>(this IEnumerable<T> source, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			var array = new T[count];
			var i = 0;
			foreach (var item in source)
			{
				array[i++] = item;
			}
			return array;
		}

		public static T ToObject<T>(this JsonElement element, JsonSerializerOptions options = null)
		{
			var json = element.GetRawText();
			return JsonSerializer.Deserialize<T>(json, options);
		}

		public static T ToObject<T>(this JsonDocument document, JsonSerializerOptions options = null)
		{
			if (document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}
			return document.RootElement.ToObject<T>(options);
		}

		private static string FindProgram(string program)
		{
			program = IsWindows ? program + ".exe" : program;
			//Look through every directory and any subfolders they have called bin
			foreach (var dir in GetDirectories(program))
			{
				if (TryGetProgram(dir, program, out var path))
				{
					return path;
				}
				else if (TryGetProgram(Path.Combine(dir, "bin"), program, out path))
				{
					return path;
				}
			}
			return null;
		}

		private static IEnumerable<string> GetDirectories(string program)
		{
			static IReadOnlyList<T> GetValues<T>() where T : Enum
			{
				var uncast = Enum.GetValues(typeof(T));
				var cast = new T[uncast.Length];
				for (var i = 0; i < uncast.Length; ++i)
				{
					cast[i] = (T)uncast.GetValue(i);
				}
				return cast;
			}

			//Check where the program is stored
			if (Assembly.GetExecutingAssembly().Location is string assembly)
			{
				yield return Path.GetDirectoryName(assembly);
			}
			//Check path variables
			if (Environment.GetEnvironmentVariable("PATH") is string path)
			{
				foreach (var part in path.Split(IsWindows ? ';' : ':'))
				{
					yield return part.Trim();
				}
			}
			//Check every special folder
			foreach (var folder in GetValues<Environment.SpecialFolder>())
			{
				yield return Path.Combine(Environment.GetFolderPath(folder), program);
			}
		}

		private static bool TryGetProgram(string directory, string program, out string path)
		{
			if (!Directory.Exists(directory))
			{
				path = null;
				return false;
			}

			var files = Directory.EnumerateFiles(directory, program, SearchOption.TopDirectoryOnly);
			path = files.FirstOrDefault();
			return path != null;
		}
	}
}