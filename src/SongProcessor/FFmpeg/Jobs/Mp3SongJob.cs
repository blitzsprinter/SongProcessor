using SongProcessor.Models;

using System.Collections.Immutable;

namespace SongProcessor.FFmpeg.Jobs;

public class Mp3SongJob : SongJob
{
	protected internal static IReadOnlyDictionary<string, string> AudioArgs { get; } = new Dictionary<string, string>(Args)
	{
		["vn"] = "", // No video
		["f"] = "mp3"
	}.ToImmutableDictionary();

	public Mp3SongJob(IAnime anime, ISong song) : base(anime, song)
	{
	}

	protected internal virtual FFmpegArgs GenerateArgsInternal()
	{
		FFmpegInput[] input;
		if (Song.CleanPath is null)
		{
			input = new FFmpegInput[]
			{
				new(Anime.GetSourceFile(), new Dictionary<string, string>
				{
					["ss"] = Song.Start.ToString(), // Starting time
					["to"] = Song.End.ToString(), // Ending time
				}),
			};
		}
		else
		{
			input = new FFmpegInput[]
			{
				new(Song.GetCleanFile(Anime)!, new Dictionary<string, string>
				{
					["to"] = Song.GetLength().ToString(), // Should start at needed segment
				}),
			};
		}

		var mapping = new[]
		{
			$"0:a:{Song.OverrideAudioTrack}",
		};

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
			Args: AudioArgs,
			AudioFilters: audioFilters,
			VideoFilters: null,
			OutputFile: GetSanitizedPath()
		);
	}

	protected override string GenerateArgs()
		=> GenerateArgsInternal().ToString();

	protected override string GetUnsanitizedPath()
		=> Song.GetMp3File(Anime);
}