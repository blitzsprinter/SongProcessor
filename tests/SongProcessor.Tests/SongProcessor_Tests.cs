using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.FFmpeg;
using SongProcessor.FFmpeg.Jobs;
using SongProcessor.Models;
using SongProcessor.Tests.FFmpeg;

namespace SongProcessor.Tests;

[TestClass]
public sealed class SongProcessor_Tests : FFmpeg_TestsBase
{
	private readonly SongProcessor _Processor = new();

	[TestMethod]
	public void CreateJobsNoSongs_Test()
	{
		var actual = ((ISongProcessor)_Processor).CreateJobs(new Anime[]
		{
			CreateAnime(),
		});
		actual.Should().BeEmpty();
	}

	[TestMethod]
	public void CreateJobsSomeExisting_Test()
	{
		var anime = CreateAnime(720);
		var song = anime.Songs.Single();
		song.Status = Status.Mp3 | Status.Res720;
		var expected = new SongJob[]
		{
			new VideoSongJob(anime, anime.Songs.Single(), 480),
		};

		var actual = ((ISongProcessor)_Processor).CreateJobs(new[] { anime });
		actual.Should().BeEquivalentTo(expected);
	}

	[TestMethod]
	public void CreateJobsSongHasNoTimestamp_Test()
	{
		var actual = ((ISongProcessor)_Processor).CreateJobs(new Anime[]
		{
			CreateAnime(createSongs: () => new Song[]
			{
				new()
				{
					Start = TimeSpan.FromSeconds(0),
				},
			}),
		});
		actual.Should().BeEmpty();
	}

	[TestMethod]
	public void CreateJobsSongIsIgnored_Test()
	{
		var actual = ((ISongProcessor)_Processor).CreateJobs(new Anime[]
		{
			CreateAnime(createSongs: () => new Song[]
			{
				new()
				{
					ShouldIgnore = true,
				},
			}),
		});
		actual.Should().BeEmpty();
	}

	[TestMethod]
	public void CreateJobsVideo360p_Test()
	{
		var anime = CreateAnime(360);
		var expected = new SongJob[]
		{
			new Mp3SongJob(anime, anime.Songs.Single()),
			new VideoSongJob(anime, anime.Songs.Single(), anime.VideoInfo!.Height),
		};

		var actual = ((ISongProcessor)_Processor).CreateJobs(new[] { anime });
		actual.Should().BeEquivalentTo(expected);
	}

	[TestMethod]
	public void CreateJobsVideo480p_Test()
	{
		var anime = CreateAnime(480);
		var expected = new SongJob[]
		{
			new Mp3SongJob(anime, anime.Songs.Single()),
			new VideoSongJob(anime, anime.Songs.Single(), 480),
		};

		var actual = ((ISongProcessor)_Processor).CreateJobs(new[] { anime });
		actual.Should().BeEquivalentTo(expected);
	}

	[TestMethod]
	public void CreateJobsVideo720p_Test()
	{
		var anime = CreateAnime(720);
		var expected = new SongJob[]
		{
			new Mp3SongJob(anime, anime.Songs.Single()),
			new VideoSongJob(anime, anime.Songs.Single(), 480),
			new VideoSongJob(anime, anime.Songs.Single(), 720),
		};

		var actual = ((ISongProcessor)_Processor).CreateJobs(new[] { anime });
		actual.Should().BeEquivalentTo(expected);
	}

	[TestMethod]
	public void CreateJobsVideoIsNull_Test()
	{
		var actual = ((ISongProcessor)_Processor).CreateJobs(new Anime[]
		{
			CreateAnime(createVideoInfo: () => null),
		});
		actual.Should().BeEmpty();
	}

	private Anime CreateAnime(int height)
	{
		return CreateAnime(createSongs: () => new Song[]
		{
			new()
			{
				Start = TimeSpan.FromSeconds(1),
				End = TimeSpan.FromSeconds(2),
			},
		}, createVideoInfo: () => VideoInfo with
		{
			Height = height
		});
	}

	private Anime CreateAnime(
		Func<IEnumerable<Song>>? createSongs = null,
		Func<VideoInfo?>? createVideoInfo = null)
	{
		var animeBase = new AnimeBase();
		if (createSongs is not null)
		{
			animeBase.Songs.AddRange(createSongs());
		}
		var videoInfo = VideoInfo;
		if (createVideoInfo is not null)
		{
			videoInfo = createVideoInfo();
		}
		var file = Path.Combine(Directory.GetCurrentDirectory(), "info.amq");
		return new(file, animeBase, videoInfo);
	}
}