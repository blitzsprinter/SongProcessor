using System.Collections.Generic;

namespace AMQSongProcessor.Models
{
	public sealed class VolumeInfo
	{
		public Dictionary<int, int> Histograms { get; set; } = new();
		public double MaxVolume { get; set; }
		public double MeanVolume { get; set; }
		public int NSamples { get; set; }
	}
}