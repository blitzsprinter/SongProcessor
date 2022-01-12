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

	protected internal virtual JobArgs GenerateArgsInternal()
	{
		JobInput[] input;
		if (Song.CleanPath is null)
		{
			input = new JobInput[]
			{
				new(Anime.GetAbsoluteSourcePath(), new Dictionary<string, string>
				{
					["ss"] = Song.Start.ToString(), // Starting time
					["to"] = Song.End.ToString(), // Ending time
				}),
			};
		}
		else
		{
			input = new JobInput[]
			{
				new(Anime.GetCleanSongPath(Song)!, new Dictionary<string, string>
				{
					["to"] = Song.GetLength().ToString(), // Should start at needed segment
				}),
			};
		}

		var mapping = new[]
		{
			$"0:a:{Song.OverrideAudioTrack}",
		};

		var audioFilters = default(IReadOnlyDictionary<string, string>?);
		if (Song.VolumeModifier is not null)
		{
			audioFilters = new Dictionary<string, string>
			{
				["volume"] = Song.VolumeModifier.ToString()!,
			};
		}

		return new JobArgs(
			Inputs: input,
			Mapping: mapping,
			QualityArgs: AudioArgs,
			AudioFilters: audioFilters,
			VideoFilters: null,
			OutputFile: GetSanitizedPath()
		);
	}

	protected override string GenerateArgs()
		=> GenerateArgsInternal().ToString();

	protected override string GetUnsanitizedPath()
		=> Song.GetMp3Path(Anime.GetDirectory(), Anime.Id);
}