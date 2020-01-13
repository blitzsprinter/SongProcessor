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

		private static readonly (int Size, Status Status)[] Resolutions = new[]
		{
			(480, Status.Res480),
			(720, Status.Res720)
		};

		private static readonly (int, int) UnknownResolution = (-1, -1);

		private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			IgnoreReadOnlyProperties = true,
		};

		static Program()
		{
			Options.Converters.Add(new JsonStringEnumConverter());

			Console.SetBufferSize(Console.BufferWidth, short.MaxValue - 1);
		}

		public static async Task Main()
		{
#if true
			const string dir = @"D:\Lupin Songs not in AMQ";
#else
			string dir;
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
				else if (!File.Exists(anime.GetSourcePath()))
				{
					Console.WriteLine($"Source does not exist: {anime.Name}");
					Console.ReadLine();
					throw new ArgumentException($"{anime.Source} source does not exist.", nameof(anime.Source));
				}

				var (_, height) = await GetResolutionAsync(anime).CAF();
				foreach (var res in Resolutions)
				{
					if (res.Size > height)
					{
						Console.WriteLine($"Source is smaller than {res.Size}p: {anime.Name}");
					}
				}

				foreach (var song in anime.Songs)
				{
					if (!song.HasTimeStamp)
					{
						Console.WriteLine($"Timestamp is null: {song.Name}");
						continue;
					}

					foreach (var res in Resolutions)
					{
						if (res.Size <= height && (song.Status & res.Status) == 0)
						{
							await ProcessVideoAsync(anime, song, res.Size, height).CAF();
						}
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

			using var process = Utils.CreateProcess(Utils.FFprobe, args);

			(int, int) res = UnknownResolution;
			void OnOutputReceived(object sender, DataReceivedEventArgs args)
			{
				process.OutputDataReceived -= OnOutputReceived;

				var split = args.Data.Split('x');
				var cast = split.Select(int.Parse).ToArray(split.Length);
				res = (cast[0], cast[1]);
			}

			process.OutputDataReceived += OnOutputReceived;
			await process.RunAsync().CAF();
			return res;
		}

		private static async Task<int> ProcessVideoAsync(Anime anime, Song song, int resolution, int sourceResolution)
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
				$" -ss {song.TimeStamp}" + //Starting time
				$" -to {song.TimeStamp + song.Length}" + //Ending time
				$" -i \"{anime.GetSourcePath()}\"" + //Video source
				$" -map 0:v" + //Use the first input's video
				$" -map 0:a" + //Use the first input's audio
				$" -sn" + //No subtitles
				$" -shortest" +
				$" -c:a libopus" + //Set the audio codec to libopus
				$" -b:a 320k" + //Set the audio bitrate to 320k
				$" -c:v libvpx-vp9 " + //Set the video codec to libvpx-vp9
				$" -b:v 0" + //Specify the constant bitrate to be zero to only use the variable one
				$" -crf 20" + //Variable bitrate, 20 should look lossless
				$" -pix_fmt yuv420p"; //Set the pixel format to yuv420p

			if (resolution != sourceResolution)
			{
				args += $" -filter:v scale=-1:{resolution}"; //Make the video whatever resolution
			}

			args += $" -deadline good" +
				$" -cpu-used 1" +
				$" -tile-columns 6" +
				$" -row-mt 1" +
				$" -threads 8" + //Use 8 threads
				$" -ac 2" +
				$" \"{output}\"";

			using var process = Utils.CreateProcess(Utils.FFmpeg, args);

			process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
			process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

			return await process.RunAsync().CAF();
		}
	}
}