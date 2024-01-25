#define AV1
#define VP9

#undef AV1

using SongProcessor.Models;

using System.Collections.Immutable;

namespace SongProcessor.FFmpeg.Jobs;

public class VideoSongJob(IAnime anime, ISong song, int resolution) : SongJob(anime, song)
{
#if AV1
	private const string LIB = "libaom-av1";
#endif
#if VP9
	private const string LIB = "libvpx-vp9";
#endif

	public int Resolution { get; } = resolution;

	protected internal static IReadOnlyDictionary<string, string> VideoArgs { get; } = new Dictionary<string, string>(Args)
	{
		["c:a"] = "libopus", // Set the audio codec to libopus
		["c:v"] = LIB, // Set the video codec to whatever we're using
		["b:v"] = "0", // Constant bitrate = 0 so only the variable one is used
		["crf"] = "20", // Variable bitrate, 20 should look lossless
		["pix_fmt"] = "yuv420p", // Set the pixel format to yuv420p
		["g"] = "119", // Frames between each keyframe
		["shortest"] = "", // Stop once any the shorted input has stopped
		["deadline"] = "good",
		["cpu-used"] = "1", // With -deadline good, 0 = slow/quality, 5 = fast/sloppy
		["row-mt"] = "1", // Something to do with multithreading and vp9, runs faster
		["ac"] = "2",
	}.ToImmutableDictionary();

	protected internal virtual FFmpegArgs GenerateArgsInternal()
	{
		var input = new List<FFmpegInput>
		{
			new(Anime.GetSourceFile(), new Dictionary<string, string>
			{
				["ss"] = Song.Start.ToString(), // Starting time
				["to"] = Song.End.ToString(), // Ending time
			}),
		};

		string[] mapping;
		if (Song.CleanPath is null)
		{
			mapping =
			[
				$"0:v:{Song.OverrideVideoTrack}",
				$"0:a:{Song.OverrideAudioTrack}",
			];
		}
		else
		{
			input.Add(new(Song.GetCleanFile(Anime)!, null));
			mapping =
			[
				$"0:v:{Song.OverrideVideoTrack}",
				$"1:a:{Song.OverrideAudioTrack}",
			];
		}

		var videoFilters = default(Dictionary<string, string>?);
		if (Anime.VideoInfo is VideoInfo info
			&& (info.Height != Resolution
				|| info.SAR != AspectRatio.Square
				|| (Song.OverrideAspectRatio is AspectRatio r && info.DAR != r)
			)
		)
		{
			var dar = (Song.OverrideAspectRatio ?? info.DAR)
				?? throw new InvalidOperationException($"DAR cannot be null: {Anime.GetSourceFile()}.");

			var width = (int)(Resolution * dar.Ratio);
			// Make sure width is always even, otherwise sometimes things can break
			if (width % 2 != 0)
			{
				++width;
			}

			videoFilters = new()
			{
				["setsar"] = AspectRatio.Square.ToString(),
				["setdar"] = dar.ToString(),
				["scale"] = $"{width}:{Resolution}"
			};
		}

		var audioFilters = default(Dictionary<string, string>?);
		if (Song.VolumeModifier is not null)
		{
			audioFilters = new()
			{
				["volume"] = Song.VolumeModifier.ToString()!,
			};
		}

		return new FFmpegArgs(
			Inputs: input,
			Mapping: mapping,
			Args: VideoArgs,
			AudioFilters: audioFilters,
			VideoFilters: videoFilters,
			OutputFile: GetSanitizedPath()
		);
	}

	protected override string GenerateArgs()
			=> GenerateArgsInternal().ToString();

	protected override string GetUnsanitizedPath()
		=> Song.GetVideoFile(Anime, Resolution);
}