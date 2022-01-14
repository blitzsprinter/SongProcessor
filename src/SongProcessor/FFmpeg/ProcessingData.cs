namespace SongProcessor.FFmpeg;

public sealed record ProcessingData(
	TimeSpan Length,
	string File,
	Progress Progress
)
{
	public float Percentage => Math.Min(1f, Progress.OutTime.Ticks / (float)Length.Ticks);
	public TimeSpan Remaining => Length - Progress.OutTime;
	public string FileName => Path.GetFileName(File);
	// Progress.Speed can potentially be roughly 0, so don't use that value
	public TimeSpan CompletionETA => Remaining / Math.Max(0.001, Progress.Speed);
}