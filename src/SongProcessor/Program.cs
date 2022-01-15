using SongProcessor.FFmpeg;
using SongProcessor.Gatherers;
using SongProcessor.Models;
using SongProcessor.Utils;

using System.Text;

namespace SongProcessor;

public sealed class Program
{
	private string? _Current;

	private static async Task AddNewShowsAsync(ISongLoader loader, string directory)
	{
		var idFile = Path.Combine(directory, "new.txt");
		if (!File.Exists(idFile))
		{
			return;
		}

		var options = new SaveNewOptions
		(
			AddShowNameDirectory: true,
			AllowOverwrite: false,
			CreateDuplicateFile: false
		);
		var gatherer = new ANNGatherer();
		foreach (var id in File.ReadAllLines(idFile).Select(int.Parse))
		{
			var model = await gatherer.GetAsync(id, GatherOptions.All).ConfigureAwait(false);
			await loader.SaveNewAsync(directory, model, options).ConfigureAwait(false);
			Console.WriteLine($"Got information from ANN for {model.Name}.");
		}

		//Clear the file after getting all the information
		File.Create(idFile).Dispose();
	}

	private static void Display(IEnumerable<IAnime> anime)
	{
		const string UNKNOWN = "Unknown ";
		const string JOINER = " | ";

		static void DisplayStatusItems(Status status)
		{
			const Status ALL = Status.None | Status.Mp3 | Status.Res480 | Status.Res720;

			static void DisplayStatusItem(Status status, Status item, string rep)
			{
				Console.Write(" | ");

				var originalColor = Console.ForegroundColor;
				Console.ForegroundColor = (status & item) != 0 ? ConsoleColor.Green : ConsoleColor.Red;
				Console.Write(rep);
				Console.ForegroundColor = originalColor;
			}

			DisplayStatusItem(status, ALL, "Submitted");
			DisplayStatusItem(status, Status.Mp3, "Mp3");
			DisplayStatusItem(status, Status.Res480, "480p");
			DisplayStatusItem(status, Status.Res720, "720p");
		}

		static ConsoleColor GetBackground(Status status)
		{
			const Status VIDEO = Status.Res480 | Status.Res720;
			const Status ALL = Status.Mp3 | VIDEO;

			if (status == Status.NotSubmitted)
			{
				return ConsoleColor.DarkRed;
			}
			else if ((status & ALL) == ALL)
			{
				return ConsoleColor.DarkGreen;
			}
			else if ((status & VIDEO) != 0)
			{
				return ConsoleColor.DarkCyan;
			}
			else if ((status & ALL) != 0)
			{
				return ConsoleColor.DarkYellow;
			}
			return ConsoleColor.DarkRed;
		}

		static int GetConsoleLength(string? value)
		{
			if (value is null)
			{
				return 0;
			}

			var start = Console.CursorLeft;
			Console.Write(value);
			var end = Console.CursorLeft;
			Console.CursorLeft = start;

			return end - start;
		}

		static void ConsoleWritePadded(string value, int totalWidth)
		{
			var start = Console.CursorLeft;
			Console.Write(value.PadRight(totalWidth));
			var end = Console.CursorLeft;

			var diff = end - start - totalWidth;
			if (diff > 0)
			{
				Console.CursorLeft -= diff;
			}
		}

		var nameLen = int.MinValue;
		var artLen = int.MinValue;
		foreach (var song in anime.SelectMany(x => x.Songs))
		{
			nameLen = Math.Max(nameLen, GetConsoleLength(song.Name));
			artLen = Math.Max(artLen, GetConsoleLength(song.Artist));
		}

		var originalBackground = Console.BackgroundColor;
		foreach (var show in anime)
		{
			var text = $"[{show.Year}] [{show.Id}] {show.Name}";
			if (show.VideoInfo?.Info is VideoInfo i)
			{
				text += $" [{i.Width}x{i.Height}] [SAR: {i.SAR}] [DAR: {i.DAR}]";
			}
			Console.WriteLine(text);

			foreach (var song in show.Songs)
			{
				var ts = song.HasTimeStamp();
				Console.BackgroundColor = GetBackground(song.Status);
				Console.Write('\t');

				ConsoleWritePadded(song.Name ?? UNKNOWN, nameLen);
				Console.Write(JOINER);
				ConsoleWritePadded(song.Artist ?? UNKNOWN, artLen);
				Console.Write(JOINER);
				Console.Write(ts ? song.Start.ToString("hh\\:mm\\:ss") : UNKNOWN);
				Console.Write(JOINER);
				Console.Write(ts ? song.GetLength().ToString("mm\\:ss").PadRight(UNKNOWN.Length) : UNKNOWN);
				DisplayStatusItems(song.Status);

				Console.BackgroundColor = originalBackground;
				Console.WriteLine();
			}
		}
		Console.WriteLine();
	}

	private static Task Main()
		=> new Program().RunAsync();

	private void OnProcessingReceived(ProcessingData value)
	{
		//For each new path, add in an extra line break for readability
		var firstWrite = Interlocked.Exchange(ref _Current, value.File) != value.File;
		var finalWrite = value.Progress.IsEnd;
		if (firstWrite || finalWrite)
		{
			Console.WriteLine();
		}

		if (finalWrite)
		{
			Console.WriteLine($"Finished processing \"{value.File}\"\n");
			return;
		}

		if (!firstWrite)
		{
			Console.CursorLeft = 0;
		}

		Console.Write($"\"{value.File}\" is {value.Percentage * 100:00.0}% complete. " +
			$"ETA on completion: {value.CompletionETA}");
	}

	private async Task RunAsync()
	{
		string directory;
		Console.WriteLine("Enter a directory to process: ");
		while (true)
		{
			try
			{
				var temp = new DirectoryInfo(Console.ReadLine()!);
				if (temp.Exists)
				{
					directory = temp.FullName;
					break;
				}
			}
			catch
			{
				Console.WriteLine("Invalid directory provided; enter a valid one: ");
			}
		}
		if (OperatingSystem.IsWindows())
		{
			Console.SetBufferSize(Console.BufferWidth, short.MaxValue - 1);
		}
		Console.OutputEncoding = Encoding.UTF8;

		var loader = new SongLoader(new SourceInfoGatherer());
		await AddNewShowsAsync(loader, directory).ConfigureAwait(false);

		var animes = new SortedSet<Anime>(new AnimeComparer());
		var files = loader.GetFiles(directory);
		await foreach (var item in loader.LoadFromFilesAsync(files, 3))
		{
			var anime = new Anime(item);
			anime.Songs.RemoveAll(x => x.ShouldIgnore);
			animes.Add(anime);
		}

		Display(animes);

		var processor = new SongProcessor();
		await processor.ExportFixesAsync(animes, directory).ConfigureAwait(false);

		var jobs = processor.CreateJobs(animes);
		await foreach (var result in jobs.ProcessAsync(OnProcessingReceived))
		{
			if (result.IsSuccess == false)
			{
				throw new InvalidOperationException(result.ToString());
			}
		}
	}
}