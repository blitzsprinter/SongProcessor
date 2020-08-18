using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		static Program()
		{
			Console.SetBufferSize(Console.BufferWidth, short.MaxValue - 1);
		}

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
			var loader = new SongLoader(new SourceInfoGatherer());
			await AddNewShowsAsync(loader, dir).CAF();

			var anime = new List<IAnime>();
			await foreach (var item in loader.LoadFromDirectoryAsync(dir))
			{
				item.Songs.RemoveAll(x => x.ShouldIgnore);
				anime.Add(item);
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
			static void DisplayStatusItems(params bool[] items)
			{
				foreach (var item in items)
				{
					Console.Write(" | ");

					var originalColor = Console.ForegroundColor;
					Console.ForegroundColor = item ? ConsoleColor.Green : ConsoleColor.Red;
					Console.Write(item.ToString().PadRight(bool.FalseString.Length));
					Console.ForegroundColor = originalColor;
				}
			}

			static ConsoleColor GetBackground(bool submitted, bool mp3, bool r1, bool r2)
			{
				if (!submitted)
				{
					return ConsoleColor.DarkRed;
				}
				else if (mp3 && r1 && r2)
				{
					return ConsoleColor.DarkGreen;
				}
				else if (mp3 || r1 || r2)
				{
					return ConsoleColor.DarkYellow;
				}
				return ConsoleColor.DarkRed;
			}

			var nameLen = int.MinValue;
			var artLen = int.MinValue;
			foreach (var show in anime)
			{
				foreach (var song in show.Songs)
				{
					nameLen = Math.Max(nameLen, song.Name.Length);
					artLen = Math.Max(artLen, song.Artist.Length);
				}
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
					var submitted = song.Status != Status.NotSubmitted;
					var hasMp3 = (song.Status & Status.Mp3) != 0;
					var has480 = (song.Status & Status.Res480) != 0;
					var has720 = (song.Status & Status.Res720) != 0;

					Console.BackgroundColor = GetBackground(submitted, hasMp3, has480, has720);

					const string UNKNOWN = "Unknown ";
					var songStr = new[]
					{
						song.Name?.PadRight(nameLen) ?? UNKNOWN,
						song.Artist?.PadRight(artLen) ?? UNKNOWN,
						song.HasTimeStamp ? song.Start.ToString("hh\\:mm\\:ss") : UNKNOWN,
						song.HasTimeStamp ? song.Length.ToString("mm\\:ss") : UNKNOWN,
					}.Join(" | ");
					Console.Write("\t" + songStr);
					DisplayStatusItems(submitted, hasMp3, has480, has720);
					Console.BackgroundColor = originalBackground;
					Console.WriteLine();
				}
			}
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