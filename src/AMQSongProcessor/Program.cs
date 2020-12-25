using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Gatherers;
using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor
{
	public static class Program
	{
		private static string? _Current;

		private static async Task Main()
		{
			string dir;
#if true
			dir = @"D:\Songs not in AMQ\Not Lupin";
#else
			Console.WriteLine("Enter a directory to process: ");
			while (true)
			{
				try
				{
					var directory = new DirectoryInfo(Console.ReadLine());
					if (directory.Exists)
					{
						dir = directory.FullName;
						break;
					}
				}
				catch
				{
				}

				Console.WriteLine("Invalid directory provided; enter a valid one: ");
			}
#endif
			if (OperatingSystem.IsWindows())
			{
				Console.SetBufferSize(Console.BufferWidth, short.MaxValue - 1);
			}
			Console.OutputEncoding = Encoding.UTF8;

			var loader = new SongLoader(new SourceInfoGatherer());
			await AddNewShowsAsync(loader, dir).CAF();

			var anime = new SortedSet<Anime>(new AnimeComparer());
			await foreach (var item in loader.LoadFromDirectoryAsync(dir, 5))
			{
				var modifiable = new Anime(item);
				modifiable.Songs.RemoveAll(x => x.ShouldIgnore);
				anime.Add(modifiable);
			}

			Display(anime);

			var processor = new SongProcessor();
			processor.WarningReceived += Console.WriteLine;
			await processor.ExportFixesAsync(dir, anime).CAF();

			var jobs = processor.CreateJobs(anime);
			await jobs.ProcessAsync(OnProcessingReceived).CAF();
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

		private static void OnProcessingReceived(ProcessingData value)
		{
			//For each new path, add in an extra line break for readability
			var firstWrite = Interlocked.Exchange(ref _Current, value.Path) != value.Path;
			var finalWrite = value.Progress.IsEnd;
			if (firstWrite || finalWrite)
			{
				Console.WriteLine();
			}

			if (finalWrite)
			{
				Console.WriteLine($"Finished processing \"{value.Path}\"\n");
				return;
			}

			if (!firstWrite)
			{
				Console.CursorLeft = 0;
			}

			Console.Write($"\"{value.Path}\" is {value.Percentage * 100:00.0}% complete. " +
				$"ETA on completion: {value.CompletionETA}");
		}

		private static async Task AddNewShowsAsync(ISongLoader loader, string directory)
		{
			var idFile = Path.Combine(directory, "new.txt");
			if (!File.Exists(idFile))
			{
				return;
			}

			var options = new SaveNewOptions
			{
				AllowOverwrite = false,
				CreateDuplicateFile = false,
				AddShowNameDirectory = true,
			};
			var gatherer = new ANNGatherer();
			foreach (var id in File.ReadAllLines(idFile).Select(int.Parse))
			{
				var model = await gatherer.GetAsync(id).CAF();
				await loader.SaveAsync(directory, model, options).CAF();
				Console.WriteLine($"Got information from ANN for {model.Name}.");
			}

			//Clear the file after getting all the information
			File.Create(idFile);
		}
	}
}