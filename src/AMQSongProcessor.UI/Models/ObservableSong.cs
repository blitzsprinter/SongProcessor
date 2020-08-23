#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

using System;
using System.Collections.Generic;
using System.Diagnostics;

using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

using ReactiveUI;

namespace AMQSongProcessor.UI.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class ObservableSong : ReactiveObject, ISong
	{
		private HashSet<int> _AlsoIn = new HashSet<int>();
		private string _Artist;
		private string? _CleanPath;
		private TimeSpan _End;
		private int? _Episode;
		private bool _IsVisible = true;
		private string _Name;
		private int _OverrideAudioTrack;
		private int _OverrideVideoTrack;
		private bool _ShouldIgnore;
		private TimeSpan _Start;
		private Status _Status;
		private SongTypeAndPosition _Type;
		private VolumeModifer? _VolumeModifier;

		public HashSet<int> AlsoIn
		{
			get => _AlsoIn;
			set => this.RaiseAndSetIfChanged(ref _AlsoIn, value);
		}
		public string Artist
		{
			get => _Artist;
			set => this.RaiseAndSetIfChanged(ref _Artist, value);
		}
		public string? CleanPath
		{
			get => _CleanPath;
			set => this.RaiseAndSetIfChanged(ref _CleanPath, value);
		}
		public TimeSpan End
		{
			get => _End;
			set => this.RaiseAndSetIfChanged(ref _End, value);
		}
		public int? Episode
		{
			get => _Episode;
			set => this.RaiseAndSetIfChanged(ref _Episode, value);
		}
		public bool IsVisible
		{
			get => _IsVisible;
			set => this.RaiseAndSetIfChanged(ref _IsVisible, value);
		}
		public string Name
		{
			get => _Name;
			set => this.RaiseAndSetIfChanged(ref _Name, value);
		}
		public int OverrideAudioTrack
		{
			get => _OverrideAudioTrack;
			set => this.RaiseAndSetIfChanged(ref _OverrideAudioTrack, value);
		}
		public int OverrideVideoTrack
		{
			get => _OverrideVideoTrack;
			set => this.RaiseAndSetIfChanged(ref _OverrideVideoTrack, value);
		}
		public bool ShouldIgnore
		{
			get => _ShouldIgnore;
			set => this.RaiseAndSetIfChanged(ref _ShouldIgnore, value);
		}
		public TimeSpan Start
		{
			get => _Start;
			set => this.RaiseAndSetIfChanged(ref _Start, value);
		}
		public Status Status
		{
			get => _Status;
			set => this.RaiseAndSetIfChanged(ref _Status, value);
		}
		public SongTypeAndPosition Type
		{
			get => _Type;
			set => this.RaiseAndSetIfChanged(ref _Type, value);
		}
		public VolumeModifer? VolumeModifier
		{
			get => _VolumeModifier;
			set => this.RaiseAndSetIfChanged(ref _VolumeModifier, value);
		}
		IReadOnlySet<int> ISong.AlsoIn => AlsoIn;
		private string DebuggerDisplay => this.GetFullName();

		public ObservableSong(ISong other)
		{
			AlsoIn = new HashSet<int>(other.AlsoIn);
			Artist = other.Artist;
			CleanPath = other.CleanPath;
			End = other.End;
			Episode = other.Episode;
			Name = other.Name;
			OverrideAudioTrack = other.OverrideAudioTrack;
			OverrideVideoTrack = other.OverrideVideoTrack;
			ShouldIgnore = other.ShouldIgnore;
			Start = other.Start;
			Status = other.Status;
			Type = other.Type;
			VolumeModifier = other.VolumeModifier;
		}
	}
}