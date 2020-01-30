using System;
using System.Diagnostics;
using System.IO;
using AdvorangesUtils;

namespace LupinSongsAMQ
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Song
	{
		public const float UNKNOWN_TIMESTAMP = -1;

		public string Artist { get; set; }
		public string CleanPath { get; set; }
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
		public bool HasTimeStamp => TimeStampInSeconds != UNKNOWN_TIMESTAMP;
		public bool IsClean => CleanPath == null;
		public TimeSpan Length => TimeSpan.FromSeconds(LengthInSeconds);
		public float LengthInSeconds { get; set; } = UNKNOWN_TIMESTAMP;
		public string Name { get; set; }
		public bool ShouldIgnore { get; set; }
		public Status Status { get; set; }
		public TimeSpan TimeStamp => TimeSpan.FromSeconds(TimeStampInSeconds);
		public float TimeStampInSeconds { get; set; }
		public SongType Type { get; set; }
		public string VolumeModifier { get; set; }
		private string DebuggerDisplay => $"{Name} ({FullArtist})";

		public Song()
		{
		}

		public Song(string name, string artist, int timestamp, int length, SongType type, Status status)
		{
			Artist = artist;
			Name = name;
			TimeStampInSeconds = timestamp;
			LengthInSeconds = length;
			Type = type;
			Status = status;
		}

		public string GetMp3Path(Anime anime)
			=> GetPath(anime, $"[{anime.Id}] {Name}.mp3");

		public string GetVideoPath(Anime anime, int res)
			=> GetPath(anime, $"[{anime.Id}] {Name} [{res}p].webm");

		public bool IsMissing(Status status)
			=> (Status & status) == 0;

		public string ToString(int nameLen, int artLen)
		{
			return new[]
			{
				Name.PadRight(nameLen),
				FullArtist.PadRight(artLen),
				HasTimeStamp ? TimeStamp.ToString("hh\\:mm\\:ss") : "Unknown ",
				Length.ToString("mm\\:ss")
			}.Join(" | ");
		}

		public override string ToString() => ToString(0, 0);

		private string GetPath(Anime anime, string file)
			=> Path.Combine(anime.Directory, file);
	}
}