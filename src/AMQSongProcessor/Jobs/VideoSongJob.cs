#define AV1
#define VP9

#undef AV1

using AdvorangesUtils;

using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Jobs
{
	public class VideoSongJob : SongJob
	{
#if AV1
		private const string LIB = "libaom-av1";
#endif
#if VP9
		private const string LIB = "libvpx-vp9";
#endif

		public int Resolution { get; }

		public VideoSongJob(IAnime anime, ISong song, int resolution) : base(anime, song)
		{
			Resolution = resolution;
		}

		protected override string GenerateArgs()
		{
			const string ARGS =
				" -v level+error" + // Only output errors to stderr
				" -nostats" + // Do not output the default stats
				" -progress pipe:1" + // Output the stats to stdout in the easier to parse format
				" -sn" + // No subtitles
				" -map_metadata -1" + // No metadata
				" -map_chapters -1" + // No chapters
				" -shortest" +
				" -c:a libopus" + // Set the audio codec to libopus
				" -b:a 320k" + // Set the audio bitrate to 320k
				" -c:v " + LIB + // Set the video codec to whatever we're using
				" -b:v 0" + // Constant bitrate = 0 so only the variable one is used
				" -crf 20" + // Variable bitrate, 20 should look lossless
				" -pix_fmt yuv420p" + // Set the pixel format to yuv420p
				" -deadline good" +
				" -cpu-used 1" + // With -deadline good, 0 = slow/quality, 5 = fast/sloppy
				" -tile-columns 2" +
				" -tile-rows 2" +
				" -row-mt 1" +
				" -threads 8" +
				" -ac 2";

			var args =
				$" -ss {Song.Start}" + // Starting time
				$" -to {Song.End}" + // Ending time
				$" -i \"{Anime.GetAbsoluteSourcePath()}\""; // Video source

			if (Song.CleanPath is null)
			{
				args +=
					$" -map 0:v:{Song.OverrideVideoTrack}" + // Use the first input's video
					$" -map 0:a:{Song.OverrideAudioTrack}"; // Use the first input's audio
			}
			else
			{
				args +=
					$" -i \"{Song.GetCleanSongPath(Anime.GetDirectory())}\"" + // Audio source
					$" -map 0:v:{Song.OverrideVideoTrack}" + // Use the first input's video
					$" -map 1:a:{Song.OverrideAudioTrack}"; // Use the second input's audio
			}

			args += ARGS; // Add in the constant args, like quality + cpu usage

			// Resize video if needed
			if (Anime.VideoInfo?.Info is VideoInfo info
				&& (info.Height != Resolution
					|| info.SAR != SquareSAR
					|| (Song.OverrideAspectRatio is AspectRatio r && info.DAR != r)
				)
			)
			{
				var dar = Song.OverrideAspectRatio is AspectRatio ratio ? ratio : info.DAR;
				var videoFilterParts = new Dictionary<string, string>
				{
					["setsar"] = SquareSAR.ToString('/'),
					["setdar"] = dar.ToString('/'),
				};

				var width = (int)(Resolution * dar.Ratio);
				// Make sure width is always even, otherwise sometimes things can break
				if (width % 2 != 0)
				{
					++width;
				}

				videoFilterParts["scale"] = $"{width}:{Resolution}";

				var joined = videoFilterParts.Select(x => $"{x.Key}={x.Value}").Join(",");
				args += $" -filter:v \"{joined}\"";
			}

			if (Song.VolumeModifier != null)
			{
				args += $" -filter:a \"volume={Song.VolumeModifier}\"";
			}

			return args + $" \"{GetSanitizedPath()}\"";
		}

		protected override string GetUnsanitizedPath()
			=> Song.GetVideoPath(Anime.GetDirectory(), Anime.Id, Resolution);
	}
}