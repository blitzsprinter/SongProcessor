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

		private static readonly char[] TrimArray = new[] { '0', ':' };

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
			string dir;
#if true
			dir = @"D:\Songs not in AMQ\Lupin";
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
			var files = Directory.EnumerateFiles(dir, "*.amq", SearchOption.AllDirectories);
			var tasks = files.Select(async x =>
			{
				using var fs = new FileStream(x, FileMode.Open);

				var anime = await JsonSerializer.DeserializeAsync<Anime>(fs, Options).CAF();
				anime.Directory = Path.GetDirectoryName(x);
				anime.Songs.RemoveAll(x => x.ShouldIgnore);
				return anime;
			});
			var animes = (await Task.WhenAll(tasks).CAF()).Where(x => x != null).ToArray();

			DisplayAnimes(animes);
			ExportAnimeForSongFixes(dir, animes);
			await ProcessAnimesAsync(animes).CAF();
		}

		private static async Task ADownloadSongAsync()
		{
			var req = new HttpRequestMessage
			{
				RequestUri = new Uri("https://music.dmkt-sp.jp/trial/music"),
				Method = HttpMethod.Post,
			};

			//req.Headers.Add(":authority", "music.dmkt-sp.jp");
			//req.Headers.Add(":method", "POST");
			//req.Headers.Add(":path", "/trial/music");
			//req.Headers.Add(":scheme", "https");
			req.Headers.Add("accept", "application/json, text/javascript, */*; q=0.01");
			req.Headers.Add("accept-encoding", "gzip, deflate, br");
			req.Headers.Add("accept-language", "en-US,en;q=0.9");
			req.Headers.Add("cache-control", "no-cache");
			req.Headers.Add("cookie", "storedmusicid=6uhuaocsehsmmc60vkit8q6nog; trid=73.170.190.2331580343199335244; login_redirect_url=https%253A%252F%252Fmusic.dmkt-sp.jp%252Fproducts%252F%253Fsh%253D1004403588; __extfc=1; storedmusicid=6uhuaocsehsmmc60vkit8q6nog");
			req.Headers.Add("dnt", "1");
			req.Headers.Add("origin", "https://music.dmkt-sp.jp");
			req.Headers.Add("pragma", "no-cache");
			req.Headers.Add("referer", "https://music.dmkt-sp.jp/song/S14102310");
			req.Headers.Add("sec-fetch-mode", "cors");
			req.Headers.Add("sec-fetch-site", "same-origin");
			req.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");
			req.Headers.Add("x-requested-with", "XMLHttpRequest");

			req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{ "musicId", "14102310" },
				{ "trialType", "TYPICAL_TRACK" },
			});
			req.Content.Headers.Add("content-length", "40");

			var client = new HttpClient();
			var response = await client.SendAsync(req).CAF();
			var stream = await response.Content.ReadAsStreamAsync().CAF();

			const string path = @"D:\Songs not in AMQ\Failed Lupin\[Lupin] [2005] Angel Tactics\test.mp3";
			using var fs = new FileStream(path, FileMode.Create);

			await stream.CopyToAsync(fs).CAF();
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

					if (song.IsMissing(Status.Mp3))
					{
						await ProcessMp3Async(anime, song).CAF();
					}
					foreach (var res in Resolutions)
					{
						if (res.Size <= height && song.IsMissing(res.Status))
						{
							await ProcessVideoAsync(anime, song, res.Size, height).CAF();
						}
					}
				}
			}
		}

		private static async Task<(int, int)> GetResolutionAsync(Anime anime)
		{
			const string ARGS = "-v error" +
				" -select_streams v:0" +
				" -show_entries stream=width,height" +
				" -of csv=s=x:p=0";

			var args = ARGS +
				$" \"{anime.GetSourcePath()}\"";

			using var process = Utils.CreateProcess(Utils.FFprobe, args);

			(int, int) res = UnknownResolution;
			void OnOutputReceived(object sender, DataReceivedEventArgs args)
			{
				process.OutputDataReceived -= OnOutputReceived;

				var split = args.Data.Split('x');
				res = (int.Parse(split[0]), int.Parse(split[1]));
			}

			process.OutputDataReceived += OnOutputReceived;
			await process.RunAsync(false).CAF();
			return res;
		}

		private static async Task<int> ProcessVideoAsync(Anime anime, Song song, int resolution, int sourceResolution)
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

			if (resolution != sourceResolution)
			{
				args += $" -filter:v \"scale=-1:{resolution}\""; //Resize video if needed
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