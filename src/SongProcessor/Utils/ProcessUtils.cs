using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SongProcessor.Utils;

[Flags]
public enum OutputMode : uint
{
	Sync = 0,
	Async = 1U << 0,
}

public readonly struct Program(string file, string name)
{
	public string File { get; } = file;
	public string Name { get; } = name;

	public Process CreateProcess(string args)
		=> ProcessUtils.CreateProcess(File, args);
}

public static class ProcessUtils
{
	private static readonly IReadOnlyList<Environment.SpecialFolder> SpecialFolders
		= GetValues<Environment.SpecialFolder>();
	public static Program FFmpeg { get; } = FindProgram("ffmpeg");
	public static Program FFprobe { get; } = FindProgram("ffprobe");

	public static Process CreateProcess(string program, string args)
	{
		return new()
		{
			StartInfo = new()
			{
				Arguments = args,
				CreateNoWindow = true,
				FileName = program,
				RedirectStandardError = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
			},
			EnableRaisingEvents = true,
		};
	}

	public static Program FindProgram(string program)
	{
		program = OperatingSystem.IsWindows() ? $"{program}.exe" : program;
		//Look through every directory and any subfolders they have called bin
		foreach (var dir in GetDirectories(program))
		{
			if (TryGetProgram(dir, program, out var path))
			{
				return new Program(path, program);
			}
			else if (TryGetProgram(Path.Combine(dir, "bin"), program, out path))
			{
				return new Program(path, program);
			}
		}
		throw new InvalidOperationException($"Unable to find {program}.");
	}

	public static Process OnCancel(
		this Process process,
		EventHandler callback,
		CancellationToken? token = null)
	{
		process.CallbackGuards(callback);

		var isCanceled = 0;
		void OnCancel(object? sender, EventArgs args)
		{
			if (Interlocked.Exchange(ref isCanceled, 1) == 0)
			{
				callback.Invoke(sender, args);
			}
		}

		// If the program gets canceled, shut down the process
		Console.CancelKeyPress += OnCancel;
		// If the program gets shut down, make sure it also shuts down the process
		AppDomain.CurrentDomain.ProcessExit += OnCancel;
		// If an unhandled exception occurs, also attempt to shut down the process
		AppDomain.CurrentDomain.UnhandledException += OnCancel;
		// Same if a cancellation token is canceled
		var registration = token?.Register(() => OnCancel(token, EventArgs.Empty));

		// After the process is exited, remove all the cancel handling
		void OnExited(object? sender, EventArgs e)
		{
			process.Exited -= OnExited;
			Console.CancelKeyPress -= OnCancel;
			AppDomain.CurrentDomain.ProcessExit -= OnCancel;
			AppDomain.CurrentDomain.UnhandledException -= OnCancel;
			registration?.Dispose();
		}
		process.Exited += OnExited;

		return process;
	}

	public static Process OnComplete(this Process process, Action<int> callback)
	{
		process.CallbackGuards(callback);

		void OnExited(object? sender, EventArgs e)
		{
			process.Exited -= OnExited;
			callback.Invoke(process.ExitCode);
		}
		process.Exited += OnExited;

		return process;
	}

	public static Task<int> RunAsync(this Process process, OutputMode mode)
	{
		var tcs = new TaskCompletionSource<int>();

		process.EnableRaisingEvents = true;
		process.OnComplete(code => tcs.SetResult(code));
		process.Start();

		if ((mode & OutputMode.Async) != 0)
		{
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
		}

		return tcs.Task;
	}

	private static void CallbackGuards(
		this Process process,
		object callback,
		[CallerMemberName] string name = "")
	{
		if (callback is null)
		{
			throw new ArgumentNullException(nameof(callback), $"{name} does not accept a null callback.");
		}
		if (!process.EnableRaisingEvents)
		{
			throw new ArgumentException("Must be able to raise events.", nameof(process));
		}
	}

	private static IEnumerable<string> GetDirectories(string program)
	{
		yield return Directory.GetCurrentDirectory();
		// Check where the program is stored
		if (Assembly.GetExecutingAssembly().Location is string assembly)
		{
			yield return Path.GetDirectoryName(assembly)!;
		}
		// Check path variables
		if (Environment.GetEnvironmentVariable("PATH") is string path)
		{
			foreach (var part in path.Split(OperatingSystem.IsWindows() ? ';' : ':'))
			{
				yield return part.Trim();
			}
		}
		// Check every special folder
		foreach (var folder in SpecialFolders)
		{
			yield return Path.Combine(Environment.GetFolderPath(folder), program);
		}
	}

	private static IReadOnlyList<T> GetValues<T>() where T : Enum
	{
		var uncast = Enum.GetValues(typeof(T));
		var cast = new T[uncast.Length];
		for (var i = 0; i < uncast.Length; ++i)
		{
			cast[i] = (T)uncast.GetValue(i)!;
		}
		return cast;
	}

	private static bool TryGetProgram(
		string directory,
		string program,
		[NotNullWhen(true)] out string? file)
	{
		if (!Directory.Exists(directory))
		{
			file = null;
			return false;
		}

		var files = Directory.EnumerateFiles(directory, program, SearchOption.TopDirectoryOnly);
		file = files.FirstOrDefault();
		return file is not null;
	}
}