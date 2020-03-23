using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public static class Program
	{
		public const string FIXES_FILE = "fixes.txt";

		private static readonly (int Size, Status Status)[] Resolutions = new[]
		{
			(480, Status.Res480),
			(720, Status.Res720)
		};

		private static readonly AspectRatio SquareSAR = new AspectRatio(1, 1);

		private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			IgnoreReadOnlyProperties = true,
		};

		static Program()
		{
			Options.Converters.Add(new JsonStringEnumConverter());
			Options.Converters.Add(new AspectRatioJsonConverter());
			Options.Converters.Add(new SongTypeAndPositionJsonConverter());
			Options.Converters.Add(new TimeSpanJsonConverter());

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
			var anime = new List<Anime>();
			foreach (var file in Directory.EnumerateFiles(dir, "*.amq", SearchOption.AllDirectories))
			{
				using var fs = new FileStream(file, FileMode.Open);

				var show = await JsonSerializer.DeserializeAsync<Anime>(fs, Options).CAF();
				show.Directory = Path.GetDirectoryName(file);
				show.Songs.RemoveAll(x => x.ShouldIgnore);
				show.VideoInfo = await GetVideoInfoAsync(show).CAF();

				anime.Add(show);
			}

			Display(anime);
			await ExportFixesAsync(dir, anime).CAF();
			await ProcessAsync(anime).CAF();
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

		private static async Task ExportFixesAsync(string dir, IReadOnlyList<Anime> anime)
		{
			if (anime.Count == 0)
			{
				return;
			}

			static string FormatTimeSpan(TimeSpan ts)
			{
				var format = ts.TotalHours < 1 ? @"mm\:ss" : @"hh\:mm\:ss";
				return ts.ToString(format);
			}

			static string FormatTimestamp(Song song)
			{
				var ts = FormatTimeSpan(song.Start);
				if (song.Episode == null)
				{
					return ts;
				}
				return song.Episode.ToString() + "/" + ts;
			}

			var counts = new ConcurrentDictionary<string, List<Anime>>();
			foreach (var show in anime)
			{
				foreach (var song in show.Songs)
				{
					counts.GetOrAdd(song.FullName, _ => new List<Anime>()).Add(show);
				}
			}

			var file = Path.Combine(dir, FIXES_FILE);
			using var fs = new FileStream(file, FileMode.Create);
			using var sw = new StreamWriter(fs);

			foreach (var show in anime)
			{
				foreach (var song in show.Songs)
				{
					if (song.Status != Status.NotSubmitted)
					{
						continue;
					}

					var sb = new StringBuilder();

					sb.Append("**Anime:** ").AppendLine(show.Name);
					sb.Append("**ANNID:** ").AppendLine(show.Id.ToString());
					sb.Append("**Song Title:** ").AppendLine(song.Name);
					sb.Append("**Artist:** ").AppendLine(song.Artist);
					sb.Append("**Type:** ").AppendLine(song.Type.ToString());
					sb.Append("**Episode/Timestamp:** ").AppendLine(FormatTimestamp(song));
					sb.Append("**Length:** ").AppendLine(FormatTimeSpan(song.Length));

					var matches = counts[song.FullName];
					if (matches.Count > 1)
					{
						var others = matches
							.Where(x => x.Id != show.Id)
							.OrderBy(x => x.Id);

						sb.Append("**Duplicate found in:** ")
							.AppendLine(others.Join(x => x.Id.ToString()));
					}

					await sw.WriteAsync(sb.AppendLine()).CAF();
				}
			}
		}

		private static async Task ProcessAsync(IReadOnlyList<Anime> anime)
		{
			foreach (var show in anime)
			{
				if (show.Source == null)
				{
					Console.WriteLine($"Source is null: {show.Name}");
					continue;
				}
				else if (!File.Exists(show.GetSourcePath()))
				{
					Console.WriteLine($"Source does not exist: {show.Name}");
					Console.ReadLine();
					throw new ArgumentException($"{show.Source} source does not exist.", nameof(show.Source));
				}

				var validResolutions = new List<(int Size, Status Status)>(Resolutions.Length);
				foreach (var res in Resolutions)
				{
					if (res.Size > show.VideoInfo?.Height)
					{
						Console.WriteLine($"Source is smaller than {res.Size}p: {show.Name}");
					}
					else
					{
						validResolutions.Add(res);
					}
				}

				foreach (var song in show.Songs)
				{
					if (!song.HasTimeStamp)
					{
						Console.WriteLine($"Timestamp is null: {song.Name}");
						continue;
					}

					if (song.IsMissing(Status.Mp3))
					{
						await ProcessMp3Async(show, song).CAF();
					}
					foreach (var res in validResolutions)
					{
						if (song.IsMissing(res.Status))
						{
							await ProcessVideoAsync(show, song, res.Size).CAF();
						}
					}
				}
			}
		}

		private static async Task<VideoInfo> GetVideoInfoAsync(Anime anime)
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
			return infoJson.ToObject<VideoInfo>(Options);
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
				" -b:v 0" + //Constant bitrate = 0 so only the variable one is used
				" -crf 20" + //Variable bitrate, 20 should look lossless
				" -pix_fmt yuv420p" + //Set the pixel format to yuv420p
				" -deadline good" +
				" -cpu-used 1" +
				" -tile-columns 6" +
				" -row-mt 1" +
				" -threads 8" +
				" -ac 2";

			var args =
				$" -ss {song.Start}" + //Starting time
				$" -to {song.End}" + //Ending time
				$" -i \"{anime.GetSourcePath()}\""; //Video source

			if (song.IsClean)
			{
				args +=
					$" -map 0:v:{song.OverrideVideoTrack}" + //Use the first input's video
					$" -map 0:a:{song.OverrideAudioTrack}"; //Use the first input's audio
			}
			else
			{
				args +=
					$" -i \"{anime.GetCleanSongPath(song)}\"" + //Audio source
					$" -map 0:v:{song.OverrideVideoTrack}" + //Use the first input's video
					" -map 1:a"; //Use the second input's audio
			}

			args += ARGS; //Add in the constant args, like quality + cpu usage

			var width = -1;
			var videoFilterParts = new List<string>();
			//Resize video if needed
			if (anime.VideoInfo.SAR != SquareSAR)
			{
				videoFilterParts.Add($"setsar={SquareSAR.ToString('/')}");
				videoFilterParts.Add($"setdar={anime.VideoInfo.DAR.ToString('/')}");
				width = (int)(resolution * anime.VideoInfo.DAR.Ratio);
			}
			if (anime.VideoInfo.Height != resolution || width != -1)
			{
				videoFilterParts.Add($"scale={width}:{resolution}");
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
					$" -ss {song.Start}" + //Starting time
					$" -to {song.End}" + //Ending time
					$" -i \"{anime.GetSourcePath()}\"" + //Video source
					$" -map 0:a:{song.OverrideAudioTrack}" + //Use the first input's audio
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