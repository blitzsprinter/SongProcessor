using System.Diagnostics;

namespace SongProcessor.Models
{
	[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
	public class Song : ISong
	{
		public HashSet<int> AlsoIn { get; set; }
		public string Artist { get; set; }
		public string? CleanPath { get; set; }
		public TimeSpan End { get; set; }
		public int? Episode { get; set; }
		public string Name { get; set; }
		public AspectRatio? OverrideAspectRatio { get; set; }
		public int OverrideAudioTrack { get; set; }
		public int OverrideVideoTrack { get; set; }
		public bool ShouldIgnore { get; set; }
		public TimeSpan Start { get; set; }
		public Status Status { get; set; }
		public SongTypeAndPosition Type { get; set; }
		public VolumeModifer? VolumeModifier { get; set; }
		IReadOnlySet<int> ISong.AlsoIn => AlsoIn;
		private string DebuggerDisplay => this.GetFullName();

		public Song()
		{
			AlsoIn = new();
			Artist = null!;
			Name = null!;
		}

		public Song(ISong other)
		{
			AlsoIn = new(other.AlsoIn);
			Artist = other.Artist;
			CleanPath = other.CleanPath;
			End = other.End;
			Episode = other.Episode;
			Name = other.Name;
			OverrideAspectRatio = other.OverrideAspectRatio;
			OverrideAudioTrack = other.OverrideAudioTrack;
			OverrideVideoTrack = other.OverrideVideoTrack;
			ShouldIgnore = other.ShouldIgnore;
			Start = other.Start;
			Status = other.Status;
			Type = other.Type;
			VolumeModifier = other.VolumeModifier;
		}
	}
}