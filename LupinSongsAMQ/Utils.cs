using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using AdvorangesUtils;

namespace LupinSongsAMQ
{
	public static class Utils
	{
		public static string FFmpeg = FindProgram("ffmpeg");
		public static string FFprobe = FindProgram("ffprobe");

		public static Task<int> RunAsync(this Process process)
		{
			var tcs = new TaskCompletionSource<int>();

			process.EnableRaisingEvents = true;
			process.Exited += (s, e) => tcs.SetResult(process.ExitCode);
			process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
			process.ErrorDataReceived += (s, e) =>
			{
				if (!string.IsNullOrWhiteSpace(e.Data))
				{
					Console.WriteLine("ERR: " + e.Data);
				}
			};

			var started = process.Start();
			if (!started)
			{
				//you may allow for the process to be re-used (started = false)
				//but I'm not sure about the guarantees of the Exited event in such a case
				throw new InvalidOperationException("Could not start process: " + process);
			}

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			return tcs.Task;
		}

		private static string FindProgram(string program)
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

			var windows = Environment.OSVersion.Platform.ToString().CaseInsContains("win");
			var fullProgram = windows ? program + ".exe" : program;

			//Start with every special folder
			var directories = GetValues<Environment.SpecialFolder>().Select(e =>
			{
				var p = Path.Combine(Environment.GetFolderPath(e), program);
				return Directory.Exists(p) ? new DirectoryInfo(p) : null;
			}).Where(x => x != null).ToList();
			//Look through where the program is stored
			if (Assembly.GetExecutingAssembly().Location is string assembly)
			{
				directories.Add(new DirectoryInfo(Path.GetDirectoryName(assembly)));
			}
			//Check path variables
			foreach (var part in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(windows ? ';' : ':'))
			{
				if (!string.IsNullOrWhiteSpace(part))
				{
					directories.Add(new DirectoryInfo(part.Trim()));
				}
			}
			//Look through every directory and any subfolders they have called bin
			foreach (var dir in directories.SelectMany(x => new[] { x, new DirectoryInfo(Path.Combine(x?.FullName, "bin")) }))
			{
				if (dir?.Exists != true)
				{
					continue;
				}

				var files = dir.GetFiles(fullProgram, SearchOption.TopDirectoryOnly);
				if (files.Length > 0)
				{
					return files[0].FullName;
				}
			}
			return null;
		}
	}
}