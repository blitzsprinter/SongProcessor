using SongProcessor.Models;

namespace SongProcessor.FFmpeg.Jobs;

public class Mp3SongJob : SongJob
{
	public Mp3SongJob(IAnime anime, ISong song) : base(anime, song)
	{
	}

	protected override string GenerateArgs()
	{
		const string ARGS =
			" -v level+error" + // Only output errors to stderr
			" -nostats" + // Do not output the default stats
			" -progress pipe:1" + // Output the stats to stdout in the easier to parse format
			" -vn" + // No video
			" -map_metadata -1" + // No metadata
			" -map_chapters -1" + // No chapters
			" -f mp3" +
			" -b:a 320k";

		string args;
		if (Song.CleanPath is null)
		{
			args =
				$" -ss {Song.Start}" + // Starting time
				$" -to {Song.End}" + // Ending time
				$" -i \"{Anime.GetAbsoluteSourcePath()}\"" + // Video source
				$" -map 0:a:{Song.OverrideAudioTrack}"; // Use the first input's audio
		}
		else
		{
			args =
				$" -to {Song.GetLength()}" +
				$" -i \"{Anime.GetCleanSongPath(Song)}\"" +
				$" -map 0:a:{Song.OverrideAudioTrack}";
		}

		if (Song.VolumeModifier is not null)
		{
			args += $" -filter:a \"volume={Song.VolumeModifier}\"";
		}

		args += ARGS;
		return args + $" \"{GetSanitizedPath()}\"";
	}

	protected override string GetUnsanitizedPath()
		=> Song.GetMp3Path(Anime.GetDirectory(), Anime.Id);
}