﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Jobs;
using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public sealed class SongProcessor : ISongProcessor
	{
		private const int MP3 = -1;
		private static readonly Resolution RES_480 = new Resolution(480, Status.Res480);
		private static readonly Resolution RES_720 = new Resolution(720, Status.Res720);
		private static readonly Resolution RES_MP3 = new Resolution(MP3, Status.Mp3);
		private static readonly Resolution[] Resolutions = new[]
		{
			RES_MP3,
			RES_480,
			RES_720
		};

		public string FixesFile { get; set; } = "fixes.txt";
		public IProgress<ProcessingData>? Processing { get; set; }
		public IProgress<string>? Warnings { get; set; }

		public IReadOnlyList<ISongJob> CreateJobs(IEnumerable<Anime> anime)
		{
			var jobs = new List<ISongJob>();
			foreach (var show in anime)
			{
				if (show.Source == null)
				{
					Warnings?.Report($"Source is null: {show.Name}");
					continue;
				}
				else if (!File.Exists(show.AbsoluteSourcePath))
				{
					throw new FileNotFoundException($"{show.Name} source does not exist.", show.Source);
				}

				var resolutions = GetValidResolutions(show);
				var songs = show.Songs.Where(x =>
				{
					if (x.ShouldIgnore)
					{
						Warnings?.Report($"Is ignored: {x.Name}");
						return false;
					}
					if (!x.HasTimeStamp)
					{
						Warnings?.Report($"Timestamp is null: {x.Name}");
						return false;
					}
					return true;
				});
				var validJobs = GetJobs(resolutions, songs).Where(x =>
				{
					if (!x.AlreadyExists)
					{
						x.Processing = Processing;
					}
					return !x.AlreadyExists;
				});
				jobs.AddRange(validJobs);
			}
			return jobs;
		}

		public async Task ExportFixesAsync(string dir, IEnumerable<Anime> anime)
		{
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

			var songs = anime.SelectMany(x => x.Songs).Where(x => !x.ShouldIgnore).ToArray();
			if (songs.Length == 0)
			{
				return;
			}

			var matches = new ConcurrentDictionary<string, List<Anime>>();
			foreach (var song in songs)
			{
				matches.GetOrAdd(song.FullName, _ => new List<Anime>()).Add(song.Anime);
			}

			var file = Path.Combine(dir, FixesFile);
			using var sw = new StreamWriter(file, append: false);

			foreach (var song in songs)
			{
				if (song.Status != Status.NotSubmitted)
				{
					continue;
				}

				var sb = new StringBuilder();
				sb.Append("**Anime:** ").AppendLine(song.Anime.Name);
				sb.Append("**ANNID:** ").AppendLine(song.Anime.Id.ToString());
				sb.Append("**Song Title:** ").AppendLine(song.Name);
				sb.Append("**Artist:** ").AppendLine(song.Artist);
				sb.Append("**Type:** ").AppendLine(song.Type.ToString());
				sb.Append("**Episode/Timestamp:** ").AppendLine(FormatTimestamp(song));
				sb.Append("**Length:** ").AppendLine(FormatTimeSpan(song.Length));

				var m = matches[song.FullName];
				if (m.Count > 1)
				{
					var others = m
						.Where(x => x.Id != song.Anime.Id)
						.OrderBy(x => x.Id)
						.Join(x => x.Id.ToString());

					sb.Append("**Duplicate found in:** ").AppendLine(others);
				}

				await sw.WriteAsync(sb.AppendLine()).CAF();
			}
		}

		private IEnumerable<SongJob> GetJobs(IEnumerable<Resolution> resolutions, IEnumerable<Song> songs)
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
						yield return new Mp3SongJob(song);
					}
					else
					{
						yield return new VideoSongJob(song, resolution.Size);
					}
				}
			}
		}

		private IReadOnlyList<Resolution> GetValidResolutions(Anime anime)
		{
			var valid = new List<Resolution>(Resolutions.Length);
			foreach (var res in Resolutions)
			{
				if (anime.VideoInfo == null)
				{
					Warnings?.Report($"Video info is null {anime.Name}");
				}
				else if (res.Size > anime.VideoInfo?.Height)
				{
					Warnings?.Report($"Source is smaller than {res.Size}p: {anime.Name}");
				}
				else
				{
					valid.Add(res);
				}
			}

			//Smaller than 480p source. Just upscale it ¯\_(ツ)_/¯
			if (valid.Count == 1 && valid.Single().IsMp3)
			{
				valid.Add(RES_480);
			}
			return valid;
		}

		private readonly struct Resolution
		{
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