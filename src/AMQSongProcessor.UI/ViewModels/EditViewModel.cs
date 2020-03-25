using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

using AMQSongProcessor.Models;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

using Splat;

namespace AMQSongProcessor.UI.ViewModels
{
	//Never serialize this view/viewmodel since this data is related to folder structure
	[JsonConverter(typeof(NewtonsoftJsonSkipThis))]
	public class EditViewModel : ReactiveValidationObject<EditViewModel>, IRoutableViewModel
	{
		private readonly IScreen _HostScreen;
		private readonly Song _Song;
		private string _Artist;
		private int _AudioTrack;
		private string _AudioTrackText;
		private string _CleanPath;
		private string _End;
		private int _Episode;
		private string _EpisodeText;
		private string _Name;
		private int _SongPosition;
		private string _SongPositionText;
		private SongType _SongType;
		private string _Start;
		private int _VideoTrack;
		private string _VideoTrackText;
		private int _VolumeModifier;
		private string _VolumeModifierText;

		public static IReadOnlyCollection<SongType> SongTypes { get; } = new[]
		{
			SongType.Opening,
			SongType.Ending,
			SongType.Insert,
		};

		public string Artist
		{
			get => _Artist;
			set => this.RaiseAndSetIfChanged(ref _Artist, value);
		}

		public int AudioTrack
		{
			get => _AudioTrack;
			set => this.RaiseAndSetIfChanged(ref _AudioTrack, value);
		}

		public string AudioTrackText
		{
			get => _AudioTrackText;
			set => this.RaiseAndSetIfChanged(ref _AudioTrackText, value);
		}

		public string CleanPath
		{
			get => _CleanPath;
			set => this.RaiseAndSetIfChanged(ref _CleanPath, value);
		}

		public string End
		{
			get => _End;
			set => this.RaiseAndSetIfChanged(ref _End, value);
		}

		public int Episode
		{
			get => _Episode;
			set => this.RaiseAndSetIfChanged(ref _Episode, value);
		}

		public string EpisodeText
		{
			get => _EpisodeText;
			set => this.RaiseAndSetIfChanged(ref _EpisodeText, value);
		}

		public IScreen HostScreen => _HostScreen ?? Locator.Current.GetService<IScreen>();

		public string Name
		{
			get => _Name;
			set => this.RaiseAndSetIfChanged(ref _Name, value);
		}

		public ICommand Save { get; }

		public int SongPosition
		{
			get => _SongPosition;
			set => this.RaiseAndSetIfChanged(ref _SongPosition, value);
		}

		public string SongPositionText
		{
			get => _SongPositionText;
			set => this.RaiseAndSetIfChanged(ref _SongPositionText, value);
		}

		public SongType SongType
		{
			get => _SongType;
			set => this.RaiseAndSetIfChanged(ref _SongType, value);
		}

		public string Start
		{
			get => _Start;
			set => this.RaiseAndSetIfChanged(ref _Start, value);
		}

		public string UrlPathSegment => "/edit";

		public int VideoTrack
		{
			get => _VideoTrack;
			set => this.RaiseAndSetIfChanged(ref _VideoTrack, value);
		}

		public string VideoTrackText
		{
			get => _VideoTrackText;
			set => this.RaiseAndSetIfChanged(ref _VideoTrackText, value);
		}

		public int VolumeModifier
		{
			get => _VolumeModifier;
			set => this.RaiseAndSetIfChanged(ref _VolumeModifier, value);
		}

		public string VolumeModifierText
		{
			get => _VolumeModifierText;
			set => this.RaiseAndSetIfChanged(ref _VolumeModifierText, value);
		}

		public EditViewModel(Song song, IScreen screen = null)
		{
			_HostScreen = screen;
			_Song = song ?? throw new ArgumentNullException(nameof(song));

			Artist = _Song.Artist;
			AudioTrack = _Song.OverrideAudioTrack;
			CleanPath = _Song.CleanPath;
			End = _Song.End.ToString();
			Episode = _Song.Episode ?? 0;
			Name = _Song.Name;
			SongPosition = _Song.Type.Position ?? 0;
			SongType = _Song.Type.Type;
			Start = _Song.Start.ToString();
			VideoTrack = _Song.OverrideVideoTrack;
			VolumeModifier = _Song.VolumeModifier?.Decibels ?? 0;

			this.ValidationRule(
				x => x.Artist,
				x => !string.IsNullOrWhiteSpace(x),
				"Artist must not be null or empty.");

			this.ValidationRule(
				x => x.End,
				x => TimeSpan.TryParse(x, out _),
				"End must be a valid timespan.");

			this.ValidationRule(
				x => x.CleanPath,
				x => string.IsNullOrEmpty(x) || File.Exists(Path.Combine(song.Anime.Directory, x)),
				"Clean path does not lead to an existing file.");

			this.ValidationRule(
				x => x.Name,
				x => !string.IsNullOrWhiteSpace(x),
				"Name must not be null or empty.");

			this.ValidationRule(
				x => x.Start,
				x => TimeSpan.TryParse(x, out _),
				"Start must be a valid timespan.");

			this.ValidationRule(
				x => x.SongPositionText,
				x => int.TryParse(x, out _),
				"Song position must be a valid number.");

			this.ValidationRule(
				x => x.VideoTrackText,
				x => int.TryParse(x, out _),
				"Video track must be a valid number.");

			this.ValidationRule(
				x => x.EpisodeText,
				x => int.TryParse(x, out _),
				"Episode must be a valid number.");

			this.ValidationRule(
				x => x.AudioTrackText,
				x => int.TryParse(x, out _),
				"Audio track must be a valid number.");

			this.ValidationRule(
				x => x.VolumeModifierText,
				x => int.TryParse(x, out _),
				"Volume modifier must be a valid number.");

			Save = ReactiveCommand.Create(() =>
			{
				_Song.Artist = Artist;
				_Song.OverrideAudioTrack = AudioTrack;
				_Song.CleanPath = GetNullIfEmpty(CleanPath);
				_Song.End = TimeSpan.Parse(End);
				_Song.Episode = GetNullIfZero(Episode);
				_Song.Name = Name;
				_Song.Type = new SongTypeAndPosition(SongType, GetNullIfZero(SongPosition));
				_Song.Start = TimeSpan.Parse(Start);
				_Song.OverrideVideoTrack = VideoTrack;
				_Song.VolumeModifier = GetVolumeModifer(VolumeModifier);
			}, this.IsValid());
		}

		private static string GetNullIfEmpty(string str)
			=> string.IsNullOrEmpty(str) ? null : str;

		private static int? GetNullIfZero(int? num)
			=> num == 0 ? null : num;

		private static VolumeModifer? GetVolumeModifer(int? num)
			=> GetNullIfZero(num) == null ? default(VolumeModifer?) : VolumeModifer.FromDecibels(num.Value);
	}
}