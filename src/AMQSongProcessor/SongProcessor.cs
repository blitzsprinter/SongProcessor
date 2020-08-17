using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Jobs;
using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor
{
	public sealed class SongProcessor : ISongProcessor
	{
		public string FixesFile { get; set; } = "fixes.txt";

		public event Action<string>? WarningReceived;

		public IReadOnlyList<ISongJob> CreateJobs(IEnumerable<IAnime> animes)
		{
			var jobs = new List<ISongJob>();
			foreach (var anime in animes)
			{
				if (anime.Source == null)
				{
					WarningReceived?.Invoke($"Source is null: {anime.Name}");
					continue;
				}
				else if (!File.Exists(anime.GetAbsoluteSourcePath()))
				{
					throw new FileNotFoundException($"{anime.Name} source does not exist.", anime.Source);
				}

				var resolutions = GetValidResolutions(anime);
				var songs = anime.Songs.Where(x =>
				{
					if (x.ShouldIgnore)
					{
						WarningReceived?.Invoke($"Is ignored: {x.Name}");
						return false;
					}
					if (!x.HasTimeStamp)
					{
						WarningReceived?.Invoke($"Timestamp is null: {x.Name}");
						return false;
					}
					return true;
				});
				var validJobs = GetJobs(anime, songs, resolutions).Where(x => !x.AlreadyExists);
				jobs.AddRange(validJobs);
			}
			return jobs;
		}

		public async Task ExportFixesAsync(string dir, IEnumerable<IAnime> animes)
		{
			static string FormatTimeSpan(TimeSpan ts)
			{
				var format = ts.TotalHours < 1 ? @"mm\:ss" : @"hh\:mm\:ss";
				return $"`{ts.ToString(format)}`";
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

			var matches = new ConcurrentDictionary<string, List<IAnime>>();
			foreach (var anime in animes)
			{
				foreach (var song in anime.Songs)
				{
					if (song.ShouldIgnore)
					{
						continue;
					}

					matches.GetOrAdd(song.FullName, _ => new List<IAnime>()).Add(anime);
				}
			}
			if (matches.Count == 0)
			{
				return;
			}

			var file = Path.Combine(dir, FixesFile);
			using var sw = new StreamWriter(file, append: false);

			foreach (var anime in animes)
			{
				foreach (var song in anime.Songs)
				{
					if (song.ShouldIgnore || song.Status != Status.NotSubmitted)
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

					var others = matches[song.FullName]
						.Select(x => x.Id)
						.Concat(song.AlsoIn)
						.Where(x => x != anime.Id)
						.OrderBy(x => x)
						.Join(x => x.ToString());
					if (!string.IsNullOrWhiteSpace(others))
					{
						sb.Append("**Duplicate found in:** ").AppendLine(others);
					}

					sb.AppendLine("**I solemnly swear that I have checked that this song-anime combo isn't in the game already, and I have read and understand all the pins**");
					await sw.WriteAsync(sb.AppendLine()).CAF();
				}
			}
		}

		private IEnumerable<SongJob> GetJobs(
			IAnime anime,
			IEnumerable<Song> songs,
			IEnumerable<Resolution> resolutions)
		{
			foreach (var song in songs)
			{
				foreach (var resolution in resolutions)
				{
					if (!song.IsMissing(resolution.Status))
					{
						continue;
					}

					if (resolution.IsMp3)
					{
						yield return new Mp3SongJob(anime, song);
					}
					else
					{
						yield return new VideoSongJob(anime, song, resolution.Size);
					}
				}
			}
		}

		private IReadOnlyList<Resolution> GetValidResolutions(IAnime anime)
		{
			var valid = new List<Resolution>(Resolution.Resolutions.Length);
			foreach (var res in Resolution.Resolutions)
			{
				if (anime.VideoInfo == null)
				{
					WarningReceived?.Invoke($"Video info is null {anime.Name}");
				}
				else if (res.Size > anime.VideoInfo?.Info?.Height)
				{
					WarningReceived?.Invoke($"Source is smaller than {res.Size}p: {anime.Name}");
				}
				else
				{
					valid.Add(res);
				}
			}

			//Smaller than 480p source. Just upscale it ¯\_(ツ)_/¯
			if (valid.Count == 1 && valid.Single().IsMp3)
			{
				valid.Add(Resolution.RES_480);
			}
			return valid;
		}

		private readonly struct Resolution
		{
			public static readonly Resolution RES_480 = new Resolution(480, Status.Res480);
			public static readonly Resolution RES_720 = new Resolution(720, Status.Res720);
			public static readonly Resolution RES_MP3 = new Resolution(MP3, Status.Mp3);
			public static readonly Resolution[] Resolutions = new[]
			{
				RES_MP3,
				RES_480,
				RES_720
			};
			private const int MP3 = -1;

			public bool IsMp3 => Size == MP3;
			public int Size { get; }
			public Status Status { get; }

			private Resolution(int size, Status status)
			{
				Size = size;
				Status = status;
			}
		}
	}
}