using ReactiveUI;

using SongProcessor.Models;

using System.Runtime.Serialization;

namespace SongProcessor.UI.ViewModels;

[DataContract]
public sealed class SongVisibility : ReactiveObject
{
	private bool _IsExpanded;
	private bool _ShowCompletedSongs = true;
	private bool _ShowIgnoredSongs = true;
	private bool _ShowMissing480pSongs = true;
	private bool _ShowMissing720pSongs = true;
	private bool _ShowMissingMp3Songs = true;
	private bool _ShowUnsubmittedSongs = true;

	[DataMember]
	public bool IsExpanded
	{
		get => _IsExpanded;
		set => this.RaiseAndSetIfChanged(ref _IsExpanded, value);
	}
	[DataMember]
	public bool ShowCompletedSongs
	{
		get => _ShowCompletedSongs;
		set => this.RaiseAndSetIfChanged(ref _ShowCompletedSongs, value);
	}
	[DataMember]
	public bool ShowIgnoredSongs
	{
		get => _ShowIgnoredSongs;
		set => this.RaiseAndSetIfChanged(ref _ShowIgnoredSongs, value);
	}
	[DataMember]
	public bool ShowMissing480pSongs
	{
		get => _ShowMissing480pSongs;
		set => this.RaiseAndSetIfChanged(ref _ShowMissing480pSongs, value);
	}
	[DataMember]
	public bool ShowMissing720pSongs
	{
		get => _ShowMissing720pSongs;
		set => this.RaiseAndSetIfChanged(ref _ShowMissing720pSongs, value);
	}
	[DataMember]
	public bool ShowMissingMp3Songs
	{
		get => _ShowMissingMp3Songs;
		set => this.RaiseAndSetIfChanged(ref _ShowMissingMp3Songs, value);
	}
	[DataMember]
	public bool ShowUnsubmittedSongs
	{
		get => _ShowUnsubmittedSongs;
		set => this.RaiseAndSetIfChanged(ref _ShowUnsubmittedSongs, value);
	}

	public bool IsVisible(ISong song)
	{
		const Status COMPLETED = Status.Mp3 | Status.Res480 | Status.Res720;

		if (!ShowIgnoredSongs && song.ShouldIgnore)
		{
			return false;
		}
		if (!ShowUnsubmittedSongs && song.IsUnsubmitted())
		{
			return false;
		}

		return (ShowCompletedSongs && (song.Status & COMPLETED) == COMPLETED)
			|| (ShowMissingMp3Songs && (song.Status & Status.Mp3) == 0)
			|| (ShowMissing480pSongs && (song.Status & Status.Res480) == 0)
			|| (ShowMissing720pSongs && (song.Status & Status.Res720) == 0);
	}
}