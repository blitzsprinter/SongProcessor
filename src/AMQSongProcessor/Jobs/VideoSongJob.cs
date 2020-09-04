#define AV1
#define VP9

#undef AV1

using System;
using System.Collections.Generic;

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
				" -v quiet" +
				" -stats" +
				" -progress pipe:1" +
				" -sn" + //No subtitles
				" -map_metadata -1" + //No metadata
				" -map_chapters -1" + //No chapters
				" -shortest" +
				" -c:a libopus" + //Set the audio codec to libopus
				" -b:a 320k" + //Set the audio bitrate to 320k
				" -c:v " + LIB + //Set the video codec to whatever we're using
				" -b:v 0" + //Constant bitrate = 0 so only the variable one is used
				" -crf 20" + //Variable bitrate, 20 should look lossless
				" -pix_fmt yuv420p" + //Set the pixel format to yuv420p
				" -deadline good" +
				" -cpu-used 1" + //With -deadline good, 0 = slow/quality, 5 = fast/sloppy
				" -tile-columns 2" +
				" -tile-rows 2" +
				" -row-mt 1" +
				" -threads 8" +
				" -ac 2";

			var args =
				$" -ss {Song.Start}" + //Starting time
				$" -to {Song.End}" + //Ending time
				$" -i \"{Anime.GetAbsoluteSourcePath()}\""; //Video source

			if (Song.CleanPath == null)
			{
				args +=
					$" -map 0:v:{Song.OverrideVideoTrack}" + //Use the first input's video
					$" -map 0:a:{Song.OverrideAudioTrack}"; //Use the first input's audio
			}
			else
			{
				args +=
					$" -i \"{Song.GetCleanSongPath(Anime.GetDirectory())}\"" + //Audio source
					$" -map 0:v:{Song.OverrideVideoTrack}" + //Use the first input's video
					" -map 1:a"; //Use the second input's audio
			}

			args += ARGS; //Add in the constant args, like quality + cpu usage

			if (Anime.VideoInfo?.Info is VideoInfo info)
			{
				var width = -1;
				var videoFilterParts = new List<string>();
				//Resize video if needed
				if (info.SAR != SquareSAR)
				{
					videoFilterParts.Add($"setsar={SquareSAR.ToString('/')}");
					videoFilterParts.Add($"setdar={info.DAR.ToString('/')}");
					width = (int)(Resolution * info.DAR.Ratio);
				}
				if (info.Height != Resolution || width != -1)
				{
					videoFilterParts.Add($"scale={width}:{Resolution}");
				}
				if (videoFilterParts.Count > 0)
				{
					args += $" -filter:v \"{videoFilterParts.Join(",")}\"";
				}
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