using System;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Jobs
{
	public class Mp3SongJob : SongJob
	{
		public Mp3SongJob(Song song) : base(song)
		{
		}

		protected override string GenerateArgs()
		{
			var anime = Song.Anime;
			const string ARGS =
				" -v quiet" +
				" -stats" +
				" -progress pipe:1" +
				" -vn" + //No video
				" -f mp3" +
				" -b:a 320k";

			string args;
			if (Song.CleanPath == null)
			{
				args =
					$" -ss {Song.Start}" + //Starting time
					$" -to {Song.End}" + //Ending time
					$" -i \"{anime.AbsoluteSourcePath}\"" + //Video source
					$" -map 0:a:{Song.OverrideAudioTrack}"; //Use the first input's audio
			}
			else
			{
				args =
					$" -to {Song.Length}" +
					$" -i \"{Song.GetCleanSongPath()}\"";
			}

			if (Song.VolumeModifier != null)
			{
				args += $" -filter:a \"volume={Song.VolumeModifier}\"";
			}

			args += ARGS;
			return args + $" \"{GetValidPath()}\"";
		}

		[Obsolete]
		protected override string GetPath()
			=> Song.GetMp3Path();
	}
}