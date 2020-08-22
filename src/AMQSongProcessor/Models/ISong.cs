using System;
using System.Collections.Generic;

namespace AMQSongProcessor.Models
{
	public interface ISong
	{
		ISet<int> AlsoIn { get; }
		string Artist { get; set; }
		string? CleanPath { get; set; }
		TimeSpan End { get; set; }
		int? Episode { get; set; }
		string Name { get; set; }
		int OverrideAudioTrack { get; set; }
		int OverrideVideoTrack { get; set; }
		bool ShouldIgnore { get; set; }
		TimeSpan Start { get; set; }
		Status Status { get; set; }
		SongTypeAndPosition Type { get; set; }
		VolumeModifer? VolumeModifier { get; set; }
	}
}