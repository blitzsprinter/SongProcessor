using SongProcessor.FFmpeg;

namespace SongProcessor.Models
{
	public interface IAnime : IAnimeBase
	{
		string AbsoluteInfoPath { get; }
		SourceInfo<VideoInfo>? VideoInfo { get; }
	}
}