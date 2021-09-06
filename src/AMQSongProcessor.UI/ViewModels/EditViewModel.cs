using System.Reactive;
using System.Reactive.Linq;

using AMQSongProcessor.Models;
using AMQSongProcessor.UI.Models;
using AMQSongProcessor.Utils;

using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

namespace AMQSongProcessor.UI.ViewModels
{
	// Never serialize this view/viewmodel since this data is related to folder structure
	[JsonConverter(typeof(NewtonsoftJsonSkipThis))]
	public sealed class EditViewModel : ReactiveObject, IRoutableViewModel, IValidatableViewModel
	{
		private readonly ObservableAnime _Anime;
		private readonly ISongLoader _Loader;
		private readonly IMessageBoxManager _MessageBoxManager;
		private readonly ObservableSong _Song;
		private string _Artist;
		private AspectRatio _AspectRatio;
		private int _AudioTrack;
		private string _ButtonText = "Save";
		private string _CleanPath;
		private string _End;
		private int _Episode;
		private bool _Has480p;
		private bool _Has720p;
		private bool _HasMp3;
		private bool _IsSubmitted;
		private string _Name;
		private bool _ShouldIgnore;
		private int _SongPosition;
		private SongType _SongType;
		private string _Start;
		private int _VideoTrack;
		private int _VolumeModifier;

		public static IReadOnlyList<AspectRatio> AspectRatios { get; } = new[]
		{
			default,
			new AspectRatio(4, 3),
			new AspectRatio(16, 9),
		};
		public static IReadOnlyList<SongType> SongTypes { get; } = new[]
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
		public AspectRatio AspectRatio
		{
			get => _AspectRatio;
			set => this.RaiseAndSetIfChanged(ref _AspectRatio, value);
		}
		public int AudioTrack
		{
			get => _AudioTrack;
			set => this.RaiseAndSetIfChanged(ref _AudioTrack, value);
		}
		public string ButtonText
		{
			get => _ButtonText;
			set => this.RaiseAndSetIfChanged(ref _ButtonText, value);
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
		public bool Has480p
		{
			get => _Has480p;
			set => this.RaiseAndSetIfChanged(ref _Has480p, value);
		}
		public bool Has720p
		{
			get => _Has720p;
			set => this.RaiseAndSetIfChanged(ref _Has720p, value);
		}
		public bool HasMp3
		{
			get => _HasMp3;
			set => this.RaiseAndSetIfChanged(ref _HasMp3, value);
		}
		public IScreen HostScreen { get; }
		public bool IsSubmitted
		{
			get => _IsSubmitted;
			set => this.RaiseAndSetIfChanged(ref _IsSubmitted, value);
		}
		public string Name
		{
			get => _Name;
			set => this.RaiseAndSetIfChanged(ref _Name, value);
		}
		public bool ShouldIgnore
		{
			get => _ShouldIgnore;
			set => this.RaiseAndSetIfChanged(ref _ShouldIgnore, value);
		}
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
		public ValidationContext ValidationContext { get; } = new();
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

		#region Commands
		public ReactiveCommand<Unit, Unit> Save { get; }
		public ReactiveCommand<Unit, Unit> SelectCleanPath { get; }
		#endregion Commands

		public EditViewModel(
			IScreen screen,
			ISongLoader loader,
			IMessageBoxManager messageBoxManager,
			ObservableSong song)
		{
			HostScreen = screen ?? throw new ArgumentNullException(nameof(screen));
			_Song = song ?? throw new ArgumentNullException(nameof(song));
			_Anime = song.Parent ?? throw new ArgumentException("Parent cannot be null.", nameof(song));
			_Loader = loader ?? throw new ArgumentNullException(nameof(loader));
			_MessageBoxManager = messageBoxManager ?? throw new ArgumentNullException(nameof(messageBoxManager));

			_Artist = _Song.Artist;
			_AspectRatio = _Song.OverrideAspectRatio ?? AspectRatios[0];
			_AudioTrack = _Song.OverrideAudioTrack;
			_CleanPath = _Song.CleanPath!;
			_End = _Song.End.ToString();
			_Episode = _Song.Episode ?? 0;
			_Has480p = !song.IsMissing(Status.Res480);
			_Has720p = !song.IsMissing(Status.Res720);
			_HasMp3 = !song.IsMissing(Status.Mp3);
			_IsSubmitted = song.Status != Status.NotSubmitted;
			_Name = _Song.Name;
			_ShouldIgnore = _Song.ShouldIgnore;
			_SongPosition = _Song.Type.Position ?? 0;
			_SongType = _Song.Type.Type;
			_Start = _Song.Start.ToString();
			_VideoTrack = _Song.OverrideVideoTrack;
			_VolumeModifier = (int)(_Song.VolumeModifier?.Value ?? 0);

			this.ValidationRule(
				x => x.Artist,
				x => !string.IsNullOrWhiteSpace(x),
				"Artist must not be null or empty.");
			this.ValidationRule(
				x => x.CleanPath,
				x => string.IsNullOrEmpty(x) || File.Exists(Path.Combine(_Anime.GetDirectory(), x)),
				"Clean path must be null, empty, or lead to an existing file.");
			this.ValidationRule(
				x => x.Name,
				x => !string.IsNullOrWhiteSpace(x),
				"Name must not be null or empty.");

			var validTimes = this.WhenAnyValue(
				x => x.Start,
				x => x.End,
				(start, end) => new
				{
					ValidStart = TimeSpan.TryParse(start, out var s),
					Start = s,
					ValidEnd = TimeSpan.TryParse(end, out var e),
					End = e,
				})
				.Select(x => x.ValidStart && x.ValidEnd && x.Start <= x.End);
			this.ValidationRule(
				validTimes,
				"Invalid times supplied or start is less than end.");

			ValidationContext.ValidationStatusChange.Subscribe(x =>
			{
				ButtonText = x.IsValid ? "Save" : x.Text.ToSingleLine(" ");
			});

			Save = ReactiveCommand.CreateFromTask(PrivateSave, this.IsValid());
			SelectCleanPath = ReactiveCommand.CreateFromTask(PrivateSelectCleanPath);
		}

		private static AspectRatio? GetAspectRatio(AspectRatio ratio)
			=> ratio.Width == 0 || ratio.Height == 0 ? null : ratio;

		private static string? GetNullIfEmpty(string str)
			=> string.IsNullOrEmpty(str) ? null : str;

		private static int? GetNullIfZero(int? num)
			=> num == 0 ? null : num;

		private static VolumeModifer? GetVolumeModifer(int? num)
			=> GetNullIfZero(num) is null ? default : VolumeModifer.FromDecibels(num!.Value);

		private Status GetStatus()
		{
			var status = Status.NotSubmitted;
			if (HasMp3)
			{
				status |= Status.Mp3;
			}
			if (Has480p)
			{
				status |= Status.Res480;
			}
			if (Has720p)
			{
				status |= Status.Res720;
			}
			if (IsSubmitted)
			{
				status |= Status.Submitted;
			}
			return status;
		}

		private async Task PrivateSave()
		{
			_Song.Artist = Artist;
			_Song.OverrideAspectRatio = GetAspectRatio(AspectRatio);
			_Song.OverrideAudioTrack = AudioTrack;
			_Song.CleanPath = FileUtils.GetRelativeOrAbsolute(_Anime.GetDirectory(), GetNullIfEmpty(CleanPath));
			_Song.End = TimeSpan.Parse(End);
			_Song.Episode = GetNullIfZero(Episode);
			_Song.Name = Name;
			_Song.Type = new(SongType, GetNullIfZero(SongPosition));
			_Song.ShouldIgnore = ShouldIgnore;
			_Song.Status = GetStatus();
			_Song.Start = TimeSpan.Parse(Start);
			_Song.OverrideVideoTrack = VideoTrack;
			_Song.VolumeModifier = GetVolumeModifer(VolumeModifier);

			await _Loader.SaveAsync(_Anime.AbsoluteInfoPath, _Anime).ConfigureAwait(false);
		}

		private async Task PrivateSelectCleanPath()
		{
			var dir = _Anime.GetDirectory();
			var result = await _MessageBoxManager.GetFilesAsync(dir, "Clean Path", false).ConfigureAwait(true);
			if (result.SingleOrDefault() is not string path)
			{
				return;
			}

			CleanPath = path;
		}
	}
}