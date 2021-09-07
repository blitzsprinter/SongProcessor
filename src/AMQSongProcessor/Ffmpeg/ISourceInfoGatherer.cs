namespace AMQSongProcessor.FFmpeg
{
	public interface ISourceInfoGatherer
	{
		Task<SourceInfo<AudioInfo>> GetAudioInfoAsync(string file, int track = 0);

		Task<SourceInfo<VideoInfo>> GetVideoInfoAsync(string file, int track = 0);

		Task<SourceInfo<VolumeInfo>> GetVolumeInfoAsync(string file, int track = 0);
	}
}