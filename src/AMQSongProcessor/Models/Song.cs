using System;
using System.Collections.Generic;
using System.Diagnostics;

using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Song
	{
		public HashSet<int> AlsoIn { get; set; } = new HashSet<int>();
		public string Artist { get; set; } = null!;
		public string? CleanPath { get; set; }
		public TimeSpan End { get; set; }
		public int? Episode { get; set; }
		public string FullName => $"{Name} ({Artist})";
		public bool HasTimeStamp => Start > TimeSpan.FromSeconds(0);
		public bool IsCompleted => !IsMissing(Status.Res480 | Status.Res720);
		public bool IsIncompleted => !IsCompleted && !IsUnsubmitted;
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
		private string DebuggerDisplay => FullName;

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

		public Song DeepCopy()
		{
			var clone = (Song)MemberwiseClone();
			// AlsoIn could be kept as the same list, but could lead to unexpected issues
			// between restarts (since clones will have the same list until de/serialized)
			clone.AlsoIn = new HashSet<int>(AlsoIn);
			return clone;
		}

		public string? GetCleanSongPath(string directory)
			=> FileUtils.EnsureAbsolutePath(directory, CleanPath);

		public string GetMp3Path(string directory, int animeId)
			=> FileUtils.EnsureAbsolutePath(directory, $"[{animeId}] {Name}.mp3")!;

		public string GetVideoPath(string directory, int animeId, int resolution)
			=> FileUtils.EnsureAbsolutePath(directory, $"[{animeId}] {Name} [{resolution}p].webm")!;

		public bool IsMissing(Status status)
			=> (Status & status) == 0;

		public void SetCleanPath(string directory, string? path)
			=> CleanPath = FileUtils.StoreRelativeOrAbsolute(directory, path);
	}
}