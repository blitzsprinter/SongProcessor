namespace AMQSongProcessor.Models
{
	public sealed record VolumeInfo(
		Dictionary<int, int> Histograms,
		double MaxVolume,
		double MeanVolume,
		int NSamples
	);
}