using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Song
	{
		public static readonly TimeSpan UnknownTime = TimeSpan.FromSeconds(-1);

		[JsonIgnore]
		public Anime Anime { get; set; } = null!;

		public string Artist { get; set; } = null!;
		public string? CleanPath { get; set; }
		public TimeSpan End { get; set; }
		public int? Episode { get; set; }
		public string[] Featuring { get; set; } = Array.Empty<string>();

		public string FullArtist
		{
			get
			{
				if (Featuring?.Length > 0)
				{
					return Artist + " featuring " + string.Join(", ", Featuring);
				}
				return Artist;
			}
		}

		public string FullName => $"{Name} ({FullArtist})";
		public bool HasTimeStamp => Start != UnknownTime;
		public bool IsCompleted => !IsMissing(Status.Res480 | Status.Res720);
		public bool IsIncompleted => !(IsCompleted || IsUnsubmitted);
		public bool IsUnsubmitted => Status == Status.NotSubmitted;
		public TimeSpan Length => End - Start;
		public string Name { get; set; } = null!;
		public int OverrideAudioTrack { get; set; }
		public int OverrideVideoTrack { get; set; }
		public bool ShouldIgnore { get; set; }
		public TimeSpan Start { get; set; }
		public Status Status { get; set; }
		public SongTypeAndPosition Type { get; set; }
		public VolumeModifer? VolumeModifier { get; set; }
		private string DebuggerDisplay => $"{Name} ({FullArtist})";

		public Song()
		{
		}

		public Song(string name, string artist, TimeSpan start, TimeSpan end, SongTypeAndPosition type, Status status)
		{
			Artist = artist;
			Name = name;
			Start = start;
			End = end;
			Type = type;
			Status = status;
		}

		public string? GetCleanSongPath()
			=> Utils.GetFile(Anime.Directory, CleanPath);

		public string GetMp3Path()
			=> Utils.GetFile(Anime.Directory, $"[{Anime.Id}] {Name}.mp3")!;

		public string GetVideoPath(int resolution)
			=> Utils.GetFile(Anime.Directory, $"[{Anime.Id}] {Name} [{resolution}p].webm")!;

		public bool IsMissing(Status status)
			=> (Status & status) == 0;
	}
}