using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using AMQSongProcessor.UI.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json;
using ReactiveUI;

namespace AMQSongProcessor.UI
{
	public class NewtonsoftJsonSuspensionDriver : ISuspensionDriver
	{
		private readonly string _File;

		private readonly JsonSerializerSettings _Options = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.All,
		};

		public NewtonsoftJsonSuspensionDriver(string file)
		{
			_File = file;
		}

		public IObservable<Unit> InvalidateState()
		{
			if (File.Exists(_File))
			{
				File.Delete(_File);
			}
			return Observable.Return(Unit.Default);
		}

		public IObservable<object> LoadState()
		{
			if (!File.Exists(_File))
			{
				return Observable.Return(default(object));
			}

			var lines = File.ReadAllText(_File);
			var state = JsonConvert.DeserializeObject<object>(lines, _Options);
			return Observable.Return(state);
		}

		public IObservable<Unit> SaveState(object state)
		{
			var lines = JsonConvert.SerializeObject(state, _Options);
			File.WriteAllText(_File, lines);
			return Observable.Return(Unit.Default);
		}
	}
}