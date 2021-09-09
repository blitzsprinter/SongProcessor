namespace SongProcessor.FFmpeg
{
	public interface ISourceInfoGatherer
	{
		Task<SourceInfo<AudioInfo>> GetAudioInfoAsync(string path, int track = 0);

		Task<SourceInfo<VideoInfo>> GetVideoInfoAsync(string path, int track = 0);

		Task<SourceInfo<VolumeInfo>> GetVolumeInfoAsync(string path, int track = 0);
	}
}