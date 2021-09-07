using AMQSongProcessor.FFmpeg;

namespace AMQSongProcessor.Tests
{
	public class FakeSourceInfoGatherer : ISourceInfoGatherer
	{
		public SourceInfo<AudioInfo> AudioInfo { get; set; }
		public SourceInfo<VideoInfo> VideoInfo { get; set; }
		public SourceInfo<VolumeInfo> VolumeInfo { get; set; }

		public Task<SourceInfo<AudioInfo>> GetAudioInfoAsync(string path, int track = 0)
			=> Task.FromResult(AudioInfo);

		public Task<SourceInfo<VideoInfo>> GetVideoInfoAsync(string path, int track = 0)
			=> Task.FromResult(VideoInfo);

		public Task<SourceInfo<VolumeInfo>> GetVolumeInfoAsync(string path, int track = 0)
			=> Task.FromResult(VolumeInfo);
	}
}