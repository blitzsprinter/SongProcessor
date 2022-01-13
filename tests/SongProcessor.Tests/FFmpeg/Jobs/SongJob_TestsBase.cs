using FluentAssertions;

using SongProcessor.Models;

namespace SongProcessor.Tests.FFmpeg.Jobs;

public abstract class SongJob_TestsBase<T> : FFmpeg_TestsBase where T : ISongJob
{
	// The divisor to use for default length of the output
	// The input is around 5.37s long, so the output is around 1.34s
	public const int DIV = 4;

	protected static void AssertValidLength(double value, double expected)
		=> value.Should().BeApproximately(expected, expected * 0.03);

	protected static string GetSingleFile(string directory)
	{
		var files = Directory.GetFiles(directory);
		files.Should().ContainSingle();
		return files.Single();
	}

	protected static async Task<string> GetSingleFileProducedAsync(string directory, ISongJob job)
	{
		var result = await job.ProcessAsync().ConfigureAwait(false);
		result.IsSuccess.Should().BeTrue();
		return GetSingleFile(directory);
	}

	protected T GenerateJob(string directory,
		Action<Anime, Song>? configureSong = null,
		Func<Anime, Anime>? configureAnime = null)
	{
		var anime = new Anime(Path.Combine(directory, "info.amq"), new AnimeBase
		{
			Id = 73,
			Name = "Extremely Long Light Novel Title",
			Songs = new(),
			Source = ValidVideoPath,
			Year = 2500
		}, new(ValidVideoPath, ValidVideoInfo with
		{
			DAR = new(16, 9),
			SAR = AspectRatio.Square,
		}));
		var song = new Song()
		{
			Start = TimeSpan.FromSeconds(0),
			End = TimeSpan.FromSeconds(anime.VideoInfo!.Value.Info.Duration!.Value / DIV),
			Name = Guid.NewGuid().ToString(),
		};
		configureSong?.Invoke(anime, song);
		if (configureAnime is not null)
		{
			anime = configureAnime.Invoke(anime);
		}
		return GenerateJob(anime, song);
	}

	protected abstract T GenerateJob(Anime anime, Song song);
}