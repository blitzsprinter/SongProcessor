using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Jobs
{
	public class Mp3SongJob : SongJob
	{
		public Mp3SongJob(IAnime anime, ISong song) : base(anime, song)
		{
		}

		protected override string GenerateArgs()
		{
			const string ARGS =
				" -v quiet" +
				" -stats" +
				" -progress pipe:1" +
				" -vn" + //No video
				" -map_metadata -1" + //No metadata
				" -map_chapters -1" + //No chapters
				" -f mp3" +
				" -b:a 320k";

			string args;
			if (Song.CleanPath == null)
			{
				args =
					$" -ss {Song.Start}" + //Starting time
					$" -to {Song.End}" + //Ending time
					$" -i \"{Anime.GetAbsoluteSourcePath()}\"" + //Video source
					$" -map 0:a:{Song.OverrideAudioTrack}"; //Use the first input's audio
			}
			else
			{
				args =
					$" -to {Song.GetLength()}" +
					$" -i \"{Song.GetCleanSongPath(Anime.GetDirectory())}\"" +
					$" -map 0:a:{Song.OverrideAudioTrack}";
			}

			if (Song.VolumeModifier != null)
			{
				args += $" -filter:a \"volume={Song.VolumeModifier}\"";
			}

			args += ARGS;
			return args + $" \"{GetSanitizedPath()}\"";
		}

		protected override string GetUnsanitizedPath()
			=> Song.GetMp3Path(Anime.GetDirectory(), Anime.Id);
	}
}