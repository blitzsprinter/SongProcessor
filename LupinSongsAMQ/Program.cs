using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using AdvorangesUtils;

namespace LupinSongsAMQ
{
	public static class Program
	{
		public const string INFO_FILE = "info.amq";

		private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			IgnoreReadOnlyProperties = true,
		};

		static Program()
		{
			Options.Converters.Add(new JsonStringEnumConverter());
		}

		public static async Task Main()
		{
			var dir = @"D:\Lupin Songs not in AMQ";
#if false
			Console.WriteLine("Enter a directory to process: ");
			while (true)
			{
				try
				{
					var directory = new DirectoryInfo(Console.ReadLine());
					if (directory.Exists)
					{
						dir = directory.FullName;
					}
				}
				catch
				{
				}

				Console.WriteLine("Invalid directory provided; enter a valid one: ");
			}
#endif

			var tasks = Directory.EnumerateDirectories(dir).Select(async x =>
			{
				var info = Path.Combine(x, INFO_FILE);
				if (!File.Exists(info))
				{
					return null;
				}

				using var fs = new FileStream(info, FileMode.Open);

				var anime = await JsonSerializer.DeserializeAsync<Anime>(fs, Options).CAF();
				anime.Directory = Path.GetDirectoryName(info);
				return anime;
			});
			var animes = (await Task.WhenAll(tasks).CAF()).Where(x => x != null).ToArray();

			DisplayAnimes(animes);
			await ProcessAnimesAsync(animes).CAF();
		}

		private static void DisplayAnimes(IReadOnlyList<Anime> animes)
		{
			static void WriteBool(bool item)
			{
				var originalColor = Console.ForegroundColor;
				Console.ForegroundColor = item ? ConsoleColor.Green : ConsoleColor.Red;
				Console.Write(item.ToString().PadRight(bool.FalseString.Length));
				Console.ForegroundColor = originalColor;
			}

			static void DisplayStatusItems(params bool[] items)
			{
				foreach (var item in items)
				{
					Console.Write(" | ");
					WriteBool(item);
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
			foreach (var anime in animes)
			{
				foreach (var song in anime.Songs)
				{
					nameLen = Math.Max(nameLen, song.Name.Length);
					artLen = Math.Max(artLen, song.FullArtist.Length);
				}
			}

			var originalBackground = Console.BackgroundColor;
			foreach (var anime in animes)
			{
				Console.WriteLine($"[{anime.Year}] [{anime.Id}] {anime.Name}");

				foreach (var song in anime.Songs)
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

		private static async Task ProcessAnimesAsync(IReadOnlyList<Anime> animes)
		{
			foreach (var anime in animes)
			{
				if (anime.Source == null)
				{
					Console.WriteLine($"Source is null: {anime.Name}");
					continue;
				}
				else if (!File.Exists(anime.Source))
				{
					Console.WriteLine($"Source does not exist: {anime.Name}");
					Console.ReadLine();
					throw new ArgumentException($"{anime.Source} does not exist.", nameof(anime.Source));
				}

				var (_, height) = await GetResolutionAsync(anime).CAF();
				var resolutions = new[] { 480, 720 }.Where(x =>
				{
					var valid = x < height;
					if (!valid)
					{
						Console.WriteLine($"Source is smaller than {x}p: {anime.Name}");
					}
					return valid;
				}).ToArray();

				foreach (var song in anime.Songs)
				{
					if (!song.HasTimeStamp)
					{
						Console.WriteLine($"Timestamp is null: {song.Name}");
						continue;
					}

					foreach (var res in resolutions)
					{
						await ProcessVideoAsync(anime, song, res).CAF();
					}
				}
			}
		}

		private static async Task<(int, int)> GetResolutionAsync(Anime anime)
		{
			var args = $"-v error" +
				$" -select_streams v:0" +
				$" -show_entries stream=width,height" +
				$" -of csv=s=x:p=0" +
				$" \"{anime.GetSourcePath()}\"";

			using var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Utils.FFprobe,
					Arguments = args,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				},
			};

			(int, int) res = (-1, -1);
			void OnOutputReceived(object sender, DataReceivedEventArgs args)
			{
				process.OutputDataReceived -= OnOutputReceived;

				var split = args.Data.Split('x').Select(int.Parse).ToArray();
				res = (split[0], split[1]);
			}

			process.OutputDataReceived += OnOutputReceived;
			await process.RunAsync().CAF();
			return res;
		}

		private static async Task<int> ProcessVideoAsync(Anime anime, Song song, int resolution)
		{
			var file = $"[{anime.Name}] {song.Name} [{resolution}p].webm";
			var output = Path.Combine(anime.Directory, file);
			if (File.Exists(output))
			{
				return 0;
			}

			//ffmpeg -i <input.mkv> -ss <00:xx:xx.xxx> -to <00:xx:xx.xxx>
			//-i <cleanAudio.flac/ogg> -map 0:v -map 1:a -vcodec copy -acodec libopus
			//AMQvideo.webm

			var args = $" -y" + //Ignore overwrite confirmation
				$" -i \"{anime.GetSourcePath()}\"" +
				$" -ss {song.TimeStamp}" +
				$" -to {song.TimeStamp + song.Length}" +
				$" -f webm" + //Format webm
				$" -crf 15" + //Variable bitrate, 15 should look lossless
				$" -b:v 0" + //Specify the constant bitrate to be zero to only use the variable one
				$" -sn" + //No subtitles
				$" -filter:v scale=-1:{resolution}" + //Make the video whatever resolution
				$" -vcodec copy" +
				$" -acodec libopus" +
				$" \"{output}\"";

			using var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Utils.FFmpeg,
					Arguments = args,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				},
			};

			return await process.RunAsync().CAF();
		}
	}
}