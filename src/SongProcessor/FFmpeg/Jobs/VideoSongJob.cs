#define AV1
#define VP9

#undef AV1

using SongProcessor.Models;

using System.Collections.Immutable;

namespace SongProcessor.FFmpeg.Jobs;

public class VideoSongJob : SongJob
{
#if AV1
	private const string LIB = "libaom-av1";
#endif
#if VP9
	private const string LIB = "libvpx-vp9";
#endif

	public int Resolution { get; }

	protected static IReadOnlyDictionary<string, string> VideoArgs { get; } = new Dictionary<string, string>(Args)
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

	public VideoSongJob(IAnime anime, ISong song, int resolution) : base(anime, song)
	{
		Resolution = resolution;
	}

	protected override string GenerateArgs()
		=> GenerateArgsInternal().ToString();

	protected internal virtual JobArgs GenerateArgsInternal()
	{
		var input = new List<JobInput>
		{
			new(Anime.GetAbsoluteSourcePath(), new Dictionary<string, string>
			{
				["ss"] = Song.Start.ToString(), // Starting time
				["to"] = Song.End.ToString(), // Ending time
			}),
		};

		string[] mapping;
		if (Song.CleanPath is null)
		{
			mapping = new[]
			{
				$"0:v:{Song.OverrideVideoTrack}",
				$"0:a:{Song.OverrideAudioTrack}",
			};
		}
		else
		{
			input.Add(new(Anime.GetCleanSongPath(Song)!, null));
			mapping = new[]
			{
				$"0:v:{Song.OverrideVideoTrack}",
				$"1:a:{Song.OverrideAudioTrack}",
			};
		}

		var videoFilters = default(Dictionary<string, string>?);
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

			videoFilters = new()
			{
				["setsar"] = AspectRatio.Square.ToString('/'),
				["setdar"] = dar.Value.ToString('/'),
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

		return new JobArgs(
			Inputs: input,
			Mapping: mapping,
			QualityArgs: VideoArgs,
			AudioFilters: audioFilters,
			VideoFilters: videoFilters,
			OutputFile: GetSanitizedPath()
		);
	}

	protected override string GetUnsanitizedPath()
		=> Song.GetVideoPath(Anime.GetDirectory(), Anime.Id, Resolution);
}