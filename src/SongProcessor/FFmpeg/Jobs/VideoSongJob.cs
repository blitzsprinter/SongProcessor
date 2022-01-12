#define AV1
#define VP9

#undef AV1

using SongProcessor.Models;

namespace SongProcessor.FFmpeg.Jobs;

public class VideoSongJob : SongJob
{
#if AV1
	private const string LIB = "libaom-av1";
#endif
#if VP9
	private const string LIB = "libvpx-vp9";
#endif

	protected internal const string VIDEO_ARGS =
		ARGS +
		" -c:a libopus" + // Set the audio codec to libopus
		" -c:v " + LIB + // Set the video codec to whatever we're using
		" -b:v 0" + // Constant bitrate = 0 so only the variable one is used
		" -crf 20" + // Variable bitrate, 20 should look lossless
		" -pix_fmt yuv420p" + // Set the pixel format to yuv420p
		" -g 119" + // Frames between each keyframe
		" -shortest" + // Stop once any the shorted input has stopped
		" -deadline good" +
		" -cpu-used 1" + // With -deadline good, 0 = slow/quality, 5 = fast/sloppy
		" -row-mt 1" + // Something to do with multithreading and vp9, runs faster
		" -ac 2";

	public int Resolution { get; }

	public VideoSongJob(IAnime anime, ISong song, int resolution) : base(anime, song)
	{
		Resolution = resolution;
	}

	protected internal override string GenerateArgs()
	{
		var args =
			$" -ss {Song.Start}" + // Starting time
			$" -to {Song.End}" + // Ending time
			$" -i \"{Anime.GetAbsoluteSourcePath()}\""; // Video source

		if (Song.CleanPath is null)
		{
			args +=
				$" -map 0:v:{Song.OverrideVideoTrack}" + // Video's video
				$" -map 0:a:{Song.OverrideAudioTrack}"; // Video's audio
		}
		else
		{
			args +=
				$" -i \"{Anime.GetCleanSongPath(Song)}\"" + // Audio source
				$" -map 0:v:{Song.OverrideVideoTrack}" + // Video's video
				$" -map 1:a:{Song.OverrideAudioTrack}"; // Audio's video
		}

		args += VIDEO_ARGS; // Add in the constant args, like quality + cpu usage

		// Video modification
		if (Anime.VideoInfo?.Info is VideoInfo info
			&& (info.Height != Resolution
				|| info.SAR != AspectRatio.Square
				|| (Song.OverrideAspectRatio is AspectRatio r && info.DAR != r)
			)
		)
		{
			var dar = Song.OverrideAspectRatio is AspectRatio ratio ? ratio : info.DAR;
			if (dar is null)
			{
				throw new InvalidOperationException($"DAR cannot be null: {Anime.GetAbsoluteSourcePath()}.");
			}

			var width = (int)(Resolution * dar.Value.Ratio);
			// Make sure width is always even, otherwise sometimes things can break
			if (width % 2 != 0)
			{
				++width;
			}

			var videoFilterParts = new Dictionary<string, string>
			{
				["setsar"] = AspectRatio.Square.ToString('/'),
				["setdar"] = dar.Value.ToString('/'),
				["scale"] = $"{width}:{Resolution}"
			};
			var kvp = videoFilterParts.Select(x => $"{x.Key}={x.Value}");
			args += $" -filter:v \"{string.Join(',', kvp)}\"";
		}

		// Audio modification
		if (Song.VolumeModifier is not null)
		{
			args += $" -filter:a \"volume={Song.VolumeModifier}\"";
		}

		return args + $" \"{GetSanitizedPath()}\"";
	}

	protected override string GetUnsanitizedPath()
		=> Song.GetVideoPath(Anime.GetDirectory(), Anime.Id, Resolution);
}