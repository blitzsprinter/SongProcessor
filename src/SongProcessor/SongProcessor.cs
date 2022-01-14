using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Results;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;

namespace SongProcessor;

public sealed class SongProcessor : ISongProcessor
{
	public event Action<IResult>? WarningReceived;

	public List<SongJob> CreateJobs(IEnumerable<IAnime> animes)
	{
		var jobs = new List<SongJob>();
		foreach (var anime in animes)
		{
			if (anime.Source is null)
			{
				WarningReceived?.Invoke(new SourceIsNull(anime));
				continue;
			}
			else if (!File.Exists(anime.GetAbsoluteSourceFile()))
			{
				throw new FileNotFoundException($"{anime.Name} source does not exist.", anime.Source);
			}

			jobs.AddRange(CreateJobs(anime).Where(x => !x.AlreadyExists));
		}
		return jobs;
	}

	public string ExportFixes(IEnumerable<IAnime> animes)
	{
		static string FormatTimeSpan(TimeSpan ts)
		{
			var format = ts.TotalHours < 1 ? @"mm\:ss" : @"hh\:mm\:ss";
			return $"`{ts.ToString(format)}`";
		}

		static string FormatTimestamp(ISong song)
		{
			var ts = FormatTimeSpan(song.Start);
			if (song.Episode is null)
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
		if (matches.IsEmpty)
		{
			return "";
		}

		var count = 0;
		var sb = new StringBuilder();
		foreach (var anime in animes)
		{
			foreach (var song in anime.Songs)
			{
				if (song.ShouldIgnore || song.Status != Status.NotSubmitted)
				{
					continue;
				}

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
					.Select(x => x.ToString());
				var joined = string.Join(" ,", others);
				if (!string.IsNullOrWhiteSpace(joined))
				{
					sb.Append("**Duplicate found in:** ").AppendLine(joined);
				}

				sb.AppendLine();
				++count;
			}
		}

		var text = count > 1
			? "**I solemnly swear that I have checked that these song-anime combos aren't in the game already, and I have read and understand all the pins**"
			: "**I solemnly swear that I have checked that this song-anime combo isn't in the game already, and I have read and understand all the pins**";
		sb.Append(text);
		return sb.ToString();
	}

	IReadOnlyList<ISongJob> ISongProcessor.CreateJobs(IEnumerable<IAnime> anime)
		=> CreateJobs(anime);

	private IEnumerable<SongJob> CreateJobs(IAnime anime)
	{
		foreach (var song in anime.Songs)
		{
			if (song.ShouldIgnore)
			{
				WarningReceived?.Invoke(new IsIgnored(song));
				continue;
			}
			if (!song.HasTimeStamp())
			{
				WarningReceived?.Invoke(new TimestampIsNull(song));
				continue;
			}

			foreach (var resolution in GetValidResolutions(anime))
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
		if (!height.HasValue)
		{
			WarningReceived?.Invoke(new VideoIsNull(anime));
			return Array.Empty<Resolution>();
		}

		// Source is smaller than 480p, return mp3 and souce size (but treat as 480p status)
		if (height.Value < Resolution.RES_480.Size)
		{
			return new[]
			{
				Resolution.RES_MP3,
				new(height.Value, Status.Res480),
			};
		}
		// Source is smaller than 720p, return mp3 and 480p
		else if (height.Value < Resolution.RES_720.Size)
		{
			return Resolution.UpTo480;
		}
		// Source is at least 720p, return mp3, 480p, and 720p
		else
		{
			return Resolution.UpTo720;
		}
	}

	private readonly struct Resolution
	{
		public static readonly Resolution RES_480 = new(480, Status.Res480);
		public static readonly Resolution RES_720 = new(720, Status.Res720);
		public static readonly Resolution RES_MP3 = new(MP3, Status.Mp3);
		public static readonly IReadOnlyList<Resolution> UpTo480 = new[]
		{
			RES_MP3,
			RES_480,
		}.ToImmutableArray();
		public static readonly IReadOnlyList<Resolution> UpTo720 = new[]
		{
			RES_MP3,
			RES_480,
			RES_720
		}.ToImmutableArray();
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