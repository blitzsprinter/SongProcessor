using SongProcessor.Models;

namespace SongProcessor.FFmpeg.Jobs;

public class Mp3SongJob : SongJob
{
	protected internal const string AUDIO_ARGS =
		ARGS +
		" -vn" + // No video
		" -f mp3";

	public Mp3SongJob(IAnime anime, ISong song) : base(anime, song)
	{
	}

	protected internal override string GenerateArgs()
	{
		string args;
		if (Song.CleanPath is null)
		{
			args =
				$" -ss {Song.Start}" + // Starting time
				$" -to {Song.End}" + // Ending time
				$" -i \"{Anime.GetAbsoluteSourcePath()}\""; // Video source
		}
		else
		{
			args =
				$" -to {Song.GetLength()}" + // Clean path should start at needed segment
				$" -i \"{Anime.GetCleanSongPath(Song)}\""; // Audio source
		}
		args += $" -map 0:a:{Song.OverrideAudioTrack}"; // Input's audio

		args += AUDIO_ARGS; // Add in the constant args, like quality + cpu usage

		// Audio modification
		if (Song.VolumeModifier is not null)
		{
			args += $" -filter:a \"volume={Song.VolumeModifier}\"";
		}

		return args + $" \"{GetSanitizedPath()}\"";
	}

	protected override string GetUnsanitizedPath()
		=> Song.GetMp3Path(Anime.GetDirectory(), Anime.Id);
}