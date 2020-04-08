using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

using AMQSongProcessor.Utils;

namespace AMQSongProcessor.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Anime : INotifyPropertyChanged
	{
		private string _AbsoluteInfoPath;
		private string? _AbsoluteSourcePath;
		private string _Directory;
		private int _Id;
		private string _Name;
		private IList<Song> _Songs;
		private string? _Source;
		private VideoInfo? _VideoInfo;
		private int _Year;

		[JsonIgnore]
		public string AbsoluteInfoPath
		{
			get => _AbsoluteInfoPath;
			set
			{
				//If info file has been changed then update the directory
				if (RaiseAndSetIfChanged(ref _AbsoluteInfoPath, value))
				{
					Directory = Path.GetDirectoryName(AbsoluteInfoPath)
						?? throw new InvalidOperationException($"{nameof(AbsoluteInfoPath)} must be an absolute path.");
					AbsoluteSourcePath = FileUtils.EnsureAbsolutePath(Directory, Source);
				}
			}
		}
		[JsonIgnore]
		public string? AbsoluteSourcePath
		{
			get => _AbsoluteSourcePath;
			private set => RaiseAndSetIfChanged(ref _AbsoluteSourcePath, value);
		}
		[JsonIgnore]
		public string Directory
		{
			get => _Directory;
			private set => RaiseAndSetIfChanged(ref _Directory, value);
		}
		public int Id
		{
			get => _Id;
			set => RaiseAndSetIfChanged(ref _Id, value);
		}
		public string Name
		{
			get => _Name;
			set => RaiseAndSetIfChanged(ref _Name, value);
		}
		public IList<Song> Songs
		{
			get => _Songs;
			set => RaiseAndSetIfChanged(ref _Songs, value);
		}
		public string? Source
		{
			get => _Source;
			set
			{
				//If source has been changed then video info is not accurate any more
				if (RaiseAndSetIfChanged(ref _Source, value))
				{
					VideoInfo = null;
					AbsoluteSourcePath = FileUtils.EnsureAbsolutePath(Directory, Source);
				}
			}
		}
		[JsonIgnore]
		public VideoInfo? VideoInfo
		{
			get => _VideoInfo;
			set => RaiseAndSetIfChanged(ref _VideoInfo, value);
		}
		public int Year
		{
			get => _Year;
			set => RaiseAndSetIfChanged(ref _Year, value);
		}

		private string DebuggerDisplay => Name;

		public event PropertyChangedEventHandler? PropertyChanged;

		public Anime()
		{
			_Directory = null!;
			_AbsoluteInfoPath = null!;
			_Name = null!;
			_Songs = new SongCollection(this);
		}

		public Anime(int year, int id, string name) : this()
		{
			_Year = year;
			_Id = id;
			_Name = name;
		}

		public Anime(Anime other) : this(other.Year, other.Id, other.Name)
		{
		}

		public void SetSourceFile(string? path)
			=> Source = FileUtils.StoreRelativeOrAbsolute(Directory, path);

		private void OnPropertyChanged([CallerMemberName] string propertyName = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		private bool RaiseAndSetIfChanged<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(backingField, newValue))
			{
				return false;
			}

			backingField = newValue;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}