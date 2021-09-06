using AMQSongProcessor.FFmpeg;

namespace AMQSongProcessor.Models
{
	public interface IAnime : IAnimeBase
	{
		string AbsoluteInfoPath { get; }
		SourceInfo<VideoInfo>? VideoInfo { get; }
	}
}