namespace AMQSongProcessor.Models
{
	public interface ISong
	{
		IReadOnlySet<int> AlsoIn { get; }
		string Artist { get; }
		string? CleanPath { get; }
		TimeSpan End { get; }
		int? Episode { get; }
		string Name { get; }
		AspectRatio? OverrideAspectRatio { get; }
		int OverrideAudioTrack { get; }
		int OverrideVideoTrack { get; }
		bool ShouldIgnore { get; }
		TimeSpan Start { get; }
		Status Status { get; }
		SongTypeAndPosition Type { get; }
		VolumeModifer? VolumeModifier { get; }
	}
}