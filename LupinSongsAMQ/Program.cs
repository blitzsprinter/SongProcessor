using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using AdvorangesUtils;

namespace LupinSongsAMQ
{
	public static class Program
	{
		public const string INFO_FILE = "info.amq";
		public const string FIXES_FILE = "fixes.txt";

		private static readonly (int Size, Status Status)[] Resolutions = new[]
		{
			(480, Status.Res480),
			(720, Status.Res720)
		};

		private static readonly AspectRatio OptimalAspectRatio = new AspectRatio(16, 9);

		private static readonly char[] TrimArray = new[] { '0', ':' };

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
			string dir;
#if true
			dir = @"D:\Songs not in AMQ\Failed Lupin";
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
			var animes = new List<Anime>();
			foreach (var file in Directory.EnumerateFiles(dir, "*.amq", SearchOption.AllDirectories))
			{
				using var fs = new FileStream(file, FileMode.Open);

				var anime = await JsonSerializer.DeserializeAsync<Anime>(fs, Options).CAF();
				anime.Directory = Path.GetDirectoryName(file);
				anime.Songs.RemoveAll(x => x.ShouldIgnore);

				anime.SourceInfo = await GetVideoInfo(anime).CAF();
				animes.Add(anime);
			}

			DisplayAnimes(animes);
			ExportAnimeForSongFixes(dir, animes);
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
				var info = anime.SourceInfo;
				if (info != null)
				{
					Console.WriteLine($"[{info.Width}x{info.Height}] [{info.SAR}] [{info.DAR}]");
				}

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

		private static void ExportAnimeForSongFixes(string dir, IReadOnlyList<Anime> animes)
		{
			if (animes.Count == 0)
			{
				return;
			}

			static string FormatTimeSpan(TimeSpan ts)
				=> ts.ToString().TrimStart(TrimArray).Split('.')[0];

			static string FormatTimestamp(Song song)
			{
				var ts = FormatTimeSpan(song.TimeStamp);
				if (song.Episode == null)
				{
					return ts;
				}
				return song.Episode.ToString() + "/" + ts;
			}

			var counts = new ConcurrentDictionary<string, List<Anime>>();
			foreach (var anime in animes)
			{
				foreach (var song in anime.Songs)
				{
					counts.GetOrAdd(song.FullName, _ => new List<Anime>()).Add(anime);
				}
			}

			var file = Path.Combine(dir, FIXES_FILE);
			using var fs = new FileStream(file, FileMode.Create);
			using var sw = new StreamWriter(fs);

			foreach (var anime in animes)
			{
				foreach (var song in anime.Songs)
				{
					if (song.Status != Status.NotSubmitted)
					{
						continue;
					}

					var sb = new StringBuilder();

					sb.Append("**Anime:** ").AppendLine(anime.Name);
					sb.Append("**ANNID:** ").AppendLine(anime.Id.ToString());
					sb.Append("**Song Title:** ").AppendLine(song.Name);
					sb.Append("**Artist:** ").AppendLine(song.Artist);
					sb.Append("**Type:** ").AppendLine(song.Type.ToString());
					sb.Append("**Episode/Timestamp:** ").AppendLine(FormatTimestamp(song));
					sb.Append("**Length:** ").AppendLine(FormatTimeSpan(song.Length));

					var matches = counts[song.FullName];
					if (matches.Count > 1)
					{
						var others = matches
							.Where(x => x.Id != anime.Id)
							.OrderBy(x => x.Id);

						sb.Append("**Duplicate found in:** ")
							.AppendLine(others.Join(x => x.Id.ToString()));
					}

					sw.Write(sb.AppendLine().ToString());
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

				var validResolutions = new List<(int Size, Status Status)>(Resolutions.Length);
				foreach (var res in Resolutions)
				{
					if (res.Size > anime.SourceInfo?.Height)
					{
						Console.WriteLine($"Source is smaller than {res.Size}p: {anime.Name}");
					}
					else
					{
						validResolutions.Add(res);
					}
				}

				foreach (var song in anime.Songs)
				{
					if (!song.HasTimeStamp)
					{
						Console.WriteLine($"Timestamp is null: {song.Name}");
						continue;
					}

					if (song.IsMissing(Status.Mp3))
					{
						await ProcessMp3Async(anime, song).CAF();
					}
					foreach (var res in validResolutions)
					{
						if (song.IsMissing(res.Status))
						{
							await ProcessVideoAsync(anime, song, res.Size).CAF();
						}
					}
				}
			}
		}

		private static async Task<FfProbeInfo> GetVideoInfo(Anime anime)
		{
			var path = anime.GetSourcePath();
			if (path == null)
			{
				return null;
			}

			#region Args
			const string ARGS = "-v quiet" +
				" -select_streams v:0" +
				" -print_format json" +
				" -show_streams";

			var args = ARGS +
				$" \"{path}\"";
			#endregion Args

			using var process = Utils.CreateProcess(Utils.FFprobe, args);

			var sb = new StringBuilder();
			void OnOutputReceived(object sender, DataReceivedEventArgs args)
				=> sb.Append(args.Data);

			process.OutputDataReceived += OnOutputReceived;
			await process.RunAsync(false).CAF();
			process.OutputDataReceived -= OnOutputReceived;

			using var doc = JsonDocument.Parse(sb.ToString());

			var infoJson = doc.RootElement.GetProperty("streams")[0];
			return infoJson.ToObject<FfProbeInfo>(Options);
		}

		private static async Task<int> ProcessVideoAsync(Anime anime, Song song, int resolution)
		{
			var output = song.GetVideoPath(anime, resolution);
			if (File.Exists(output))
			{
				return 0;
			}

			#region Args
			const string ARGS =
				" -sn" + //No subtitles
				" -shortest" +
				" -c:a libopus" + //Set the audio codec to libopus
				" -b:a 320k" + //Set the audio bitrate to 320k
				" -c:v libvpx-vp9 " + //Set the video codec to libvpx-vp9
				" -b:v 0" + //Specify the constant bitrate to be zero to only use the variable one
				" -crf 20" + //Variable bitrate, 20 should look lossless
				" -pix_fmt yuv420p" + //Set the pixel format to yuv420p
				" -deadline good" +
				" -cpu-used 1" +
				" -tile-columns 6" +
				" -row-mt 1" +
				" -threads 8" + //Use 8 threads
				" -ac 2";

			var args =
				$" -ss {song.TimeStamp}" + //Starting time
				$" -to {song.TimeStamp + song.Length}" + //Ending time
				$" -i \"{anime.GetSourcePath()}\""; //Video source

			if (song.IsClean)
			{
				args +=
					" -map 0:v" + //Use the first input's video
					" -map 0:a"; //Use the first input's audio;
			}
			else
			{
				args +=
					$" -i \"{anime.GetCleanSongPath(song)}\"" +
					" -map 0:v" + //Use the first input's video
					" -map 1:a"; //Use the second input's audio
			}

			args += ARGS; //Add in the constant args, like quality + cpu usage

			var videoFilterParts = new List<string>();
			//Resize video if needed
			if (anime.SourceInfo.Height != resolution)
			{
				videoFilterParts.Add($"scale=-1:{resolution}");
			}
			if (anime.SourceInfo.DAR != OptimalAspectRatio)
			{
				videoFilterParts.Add($"setdar={OptimalAspectRatio.ToString('/')}");
			}
			if (videoFilterParts.Count > 0)
			{
				args += $" -filter:v \"{videoFilterParts.Join(",")}\"";
			}

			if (song.VolumeModifier != null)
			{
				args += $" -filter:a \"volume={song.VolumeModifier}\"";
			}

			args += $" \"{output}\"";
			#endregion Args

			using var process = Utils.CreateProcess(Utils.FFmpeg, args);

			return await process.RunAsync(true).CAF();
		}

		private static async Task<int> ProcessMp3Async(Anime anime, Song song)
		{
			var output = song.GetMp3Path(anime);
			if (File.Exists(output))
			{
				return 0;
			}

			#region Args
			const string ARGS =
				" -f mp3" +
				" -b:a 320k";

			string args;
			if (song.IsClean)
			{
				args =
					$" -ss {song.TimeStamp}" + //Starting time
					$" -to {song.TimeStamp + song.Length}" + //Ending time
					$" -i \"{anime.GetSourcePath()}\"" + //Video source
					" -vn";
			}
			else
			{
				args =
					$" -to {song.Length}" +
					$" -i \"{anime.GetCleanSongPath(song)}\"";
			}

			if (song.VolumeModifier != null)
			{
				args += $" -filter:a \"volume={song.VolumeModifier}\"";
			}

			args += ARGS;
			args += $" \"{output}\"";
			#endregion Args

			using var process = Utils.CreateProcess(Utils.FFmpeg, args);

			return await process.RunAsync(true).CAF();
		}
	}
}