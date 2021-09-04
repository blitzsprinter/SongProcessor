
using AMQSongProcessor.Models;

namespace AMQSongProcessor.Ffmpeg
{
	public interface ISourceInfoGatherer
	{
		Task<SourceInfo<AudioInfo>> GetAudioInfoAsync(string file, int track = 0);

		Task<SourceInfo<VolumeInfo>> GetAverageVolumeAsync(string file);

		Task<SourceInfo<VideoInfo>> GetVideoInfoAsync(string file, int track = 0);
	}
}