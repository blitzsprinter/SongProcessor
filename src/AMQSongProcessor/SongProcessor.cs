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
using AMQSongProcessor.Warnings;

namespace AMQSongProcessor
{
	public sealed class SongProcessor : ISongProcessor
	{
		public string FixesFile { get; set; } = "fixes.txt";

		public event Action<IWarning>? WarningReceived;

		public IReadOnlyList<ISongJob> CreateJobs(IEnumerable<IAnime> animes)
		{
			var jobs = new List<ISongJob>();
			foreach (var anime in animes)
			{
				if (anime.Source == null)
				{
					WarningReceived?.Invoke(new SourceIsNull(anime));
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
						WarningReceived?.Invoke(new IsIgnored(x));
						return false;
					}
					if (!x.HasTimeStamp())
					{
						WarningReceived?.Invoke(new TimestampIsNull(x));
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

			static string FormatTimestamp(ISong song)
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

					matches.GetOrAdd(song.GetFullName(), _ => new List<IAnime>()).Add(anime);
				}
			}
			if (matches.Count == 0)
			{
				return;
			}

			var file = Path.Combine(dir, FixesFile);
			using var sw = new StreamWriter(file, append: false);

			var count = 0;
			foreach (var anime in animes)
			{
				var writtenSongs = 0;
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
					sb.Append("**Length:** ").AppendLine(FormatTimeSpan(song.GetLength()));

					var others = matches[song.GetFullName()]
						.Select(x => x.Id)
						.Concat(song.AlsoIn)
						.Where(x => x != anime.Id)
						.OrderBy(x => x)
						.Join(x => x.ToString());
					if (!string.IsNullOrWhiteSpace(others))
					{
						sb.Append("**Duplicate found in:** ").AppendLine(others);
					}

					await sw.WriteAsync(sb.AppendLine()).CAF();
					++writtenSongs;
				}

				if (writtenSongs > 0)
				{
					++count;
				}
			}

			var text = count > 1
				? "**I solemnly swear that I have checked that these song-anime combos aren't in the game already, and I have read and understand all the pins**"
				: "**I solemnly swear that I have checked that this song-anime combo isn't in the game already, and I have read and understand all the pins**";
			await sw.WriteAsync(text).CAF();
		}

		private IEnumerable<SongJob> GetJobs(
			IAnime anime,
			IEnumerable<ISong> songs,
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
			var height = anime.VideoInfo?.Info?.Height;
			var valid = new List<Resolution>(Resolution.Resolutions.Length);
			foreach (var res in Resolution.Resolutions)
			{
				if (!height.HasValue)
				{
					WarningReceived?.Invoke(new VideoIsNull(anime));
				}
				else if (res.Size > height.Value)
				{
					WarningReceived?.Invoke(new VideoTooSmall(anime, res.Size));
				}
				else
				{
					valid.Add(res);
				}
			}

			// Only mp3 is valid, so we have to just use whatever res the source is
			if (height.HasValue && valid.Count == 1 && valid[0].IsMp3)
			{
				valid.Add(new Resolution(height.Value, Status.Res480));
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

			public Resolution(int size, Status status)
			{
				Size = size;
				Status = status;
			}
		}
	}
}