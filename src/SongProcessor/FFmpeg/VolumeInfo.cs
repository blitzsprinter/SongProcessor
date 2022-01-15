namespace SongProcessor.FFmpeg;

public sealed record VolumeInfo(
	string File,
	Dictionary<int, int> Histograms,
	double MaxVolume,
	double MeanVolume,
	int NSamples
) : SourceInfo(File);