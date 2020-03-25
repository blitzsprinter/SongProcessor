using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

using AMQSongProcessor.Models;

using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

using Splat;

namespace AMQSongProcessor.UI.ViewModels
{
	//Never serialize this view/viewmodel since this data is related to folder structure
	[JsonConverter(typeof(NewtonsoftJsonSkipThis))]
	public class EditViewModel : ReactiveObject, IRoutableViewModel, IValidatableViewModel
	{
		private readonly IScreen _HostScreen;
		private readonly Song _Song;
		private string _Artist;
		private int _AudioTrack;
		private string _CleanPath;
		private string _End;
		private int _Episode;
		private string _Name;
		private int _SongPosition;
		private SongType _SongType;
		private string _Start;
		private int _VideoTrack;
		private int _VolumeModifier;

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

		public ValidationContext ValidationContext { get; } = new ValidationContext();

		public int VideoTrack
		{
			get => _VideoTrack;
			set => this.RaiseAndSetIfChanged(ref _VideoTrack, value);
		}

		public int VolumeModifier
		{
			get => _VolumeModifier;
			set => this.RaiseAndSetIfChanged(ref _VolumeModifier, value);
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
				"Clean path must be null, empty, or lead to an existing file.");
			this.ValidationRule(
				x => x.Name,
				x => !string.IsNullOrWhiteSpace(x),
				"Name must not be null or empty.");
			this.ValidationRule(
				x => x.Start,
				x => TimeSpan.TryParse(x, out _),
				"Start must be a valid timespan.");

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