namespace SongProcessor.FFmpeg;

public interface ISourceInfoGatherer
{
	Task<AudioInfo> GetAudioInfoAsync(string file, int track = 0);

	Task<VideoInfo> GetVideoInfoAsync(string file, int track = 0);

	Task<VolumeInfo> GetVolumeInfoAsync(string file, int track = 0);
}