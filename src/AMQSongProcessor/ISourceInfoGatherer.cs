using System.Threading.Tasks;

using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public interface ISourceInfoGatherer
	{
		Task<AudioInfo> GetAudioInfoAsync(string file, int track = 0);

		Task<string> GetAverageVolumeAsync(string file);

		Task<VideoInfo> GetVideoInfoAsync(string file, int track = 0);
	}
}