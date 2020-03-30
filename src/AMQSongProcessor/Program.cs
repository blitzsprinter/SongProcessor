using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public static class Program
	{
		static Program()
		{
			Console.SetBufferSize(Console.BufferWidth, short.MaxValue - 1);
		}

		public static async Task Main()
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
			var loader = new SongLoader();
			await AddNewShowsAsync(loader, dir).CAF();
			var anime = await loader.LoadAsync(dir).ToListAsync().CAF();

			Display(anime);

			var processor = new SongProcessor
			{
				Processing = new LogProcessingToConsole(),
				Warnings = new LogWarningsToConsole(),
			};
			await processor.ExportFixesAsync(dir, anime).CAF();
			await processor.ProcessAsync(anime).CAF();
		}

		private static void Display(IReadOnlyList<Anime> anime)
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
					artLen = Math.Max(artLen, song.FullArtist.Length);
				}
			}

			var originalBackground = Console.BackgroundColor;
			foreach (var show in anime)
			{
				var text = $"[{show.Year}] [{show.Id}] {show.Name}";
				if (show.VideoInfo is VideoInfo i)
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
					Console.Write("\t" + song.ToString(nameLen, artLen));
					DisplayStatusItems(submitted, hasMp3, has480, has720);
					Console.BackgroundColor = originalBackground;
					Console.WriteLine();
				}
			}
		}

		private static async Task AddNewShowsAsync(SongLoader loader, string directory)
		{
			var idFile = Path.Combine(directory, "new.txt");
			if (!File.Exists(idFile))
			{
				return;
			}

			var ids = File.ReadAllLines(idFile).Select(int.Parse);
			await foreach (var anime in loader.LoadFromANNAsync(directory, ids))
			{
				Console.WriteLine($"Got information from ANN for {anime.Name}.");
			}

			//Clear the file after getting all the information
			File.Create(idFile);
		}
	}
}