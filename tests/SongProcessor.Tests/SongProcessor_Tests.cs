using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
			new(@"C:\info.amq", new AnimeBase(), VideoInfo),
		});
		actual.Should().BeEmpty();
	}

	[TestMethod]
	public void CreateJobsSongHasNoTimestamp_Test()
	{
		var actual = ((ISongProcessor)_Processor).CreateJobs(new Anime[]
		{
			new Anime(@"C:\info.amq", new AnimeBase()
			{
				Songs = new()
				{
					new()
					{
						Name = "notimestamp",
						Artist = "notimestamp",
						Start = TimeSpan.FromSeconds(0),
					},
				},
			}, VideoInfo)
		});
		actual.Should().BeEmpty();
	}

	[TestMethod]
	public void CreateJobsSongIsIgnored_Test()
	{
		var actual = ((ISongProcessor)_Processor).CreateJobs(new Anime[]
		{
			new Anime(@"C:\info.amq", new AnimeBase()
			{
				Songs = new()
				{
					new()
					{
						Name = "ignored",
						Artist = "ignored",
						ShouldIgnore = true,
					},
				},
			}, VideoInfo)
		});
		actual.Should().BeEmpty();
	}

	[TestMethod]
	public void CreateJobsVideo360p_Test()
	{
		var anime = new Anime(@"C:\info.amq", new AnimeBase()
		{
			Songs = new()
			{
				new()
				{
					Name = "song",
					Artist = "song",
					Start = TimeSpan.FromSeconds(1),
					End = TimeSpan.FromSeconds(2),
				},
			},
		}, new(VideoInfo.File, VideoInfo.Info with
		{
			Height = 360,
		}));
		var expected = new SongJob[]
		{
			new Mp3SongJob(anime, anime.Songs.Single()),
			new VideoSongJob(anime, anime.Songs.Single(), anime.VideoInfo!.Value.Info.Height),
		};

		var actual = ((ISongProcessor)_Processor).CreateJobs(new[] { anime });
		actual.Should().BeEquivalentTo(expected);
	}

	[TestMethod]
	public void CreateJobsVideo480p_Test()
	{
	}

	[TestMethod]
	public void CreateJobsVideo720p_Test()
	{
	}

	[TestMethod]
	public void CreateJobsVideoIsNull_Test()
	{
		var actual = ((ISongProcessor)_Processor).CreateJobs(new Anime[]
		{
			new(@"C:\info.amq", new AnimeBase(), null),
		});
		actual.Should().BeEmpty();
	}
}