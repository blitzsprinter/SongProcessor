using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

using AMQSongProcessor.UI.ViewModels;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using ReactiveUI;

namespace AMQSongProcessor.UI
{
	public class NewtonsoftJsonSuspensionDriver : ISuspensionDriver
	{
		private readonly string _File;

		private readonly JsonSerializerSettings _Options = new()
		{
			TypeNameHandling = TypeNameHandling.All,
			Formatting = Formatting.Indented,
			ContractResolver = new WritablePropertiesOnlyResolver(),
		};
		public bool DeleteOnInvalidState { get; set; }

		public NewtonsoftJsonSuspensionDriver(string file)
		{
			_File = file;
			_Options.Converters.Add(new Test());
			_Options.Converters.Add(new Test2());
			_Options.Converters.Add(new Test3());
		}

		public IObservable<Unit> InvalidateState()
		{
			if (DeleteOnInvalidState && File.Exists(_File))
			{
				File.Delete(_File);
			}
			return Observable.Return(Unit.Default);
		}

		public IObservable<object> LoadState()
		{
			if (!File.Exists(_File))
			{
				return Observable.Return(default(object))!;
			}

			var lines = File.ReadAllText(_File);
			var state = JsonConvert.DeserializeObject<object>(lines, _Options);
			return Observable.Return(state)!;
		}

		public IObservable<Unit> SaveState(object state)
		{
			var lines = JsonConvert.SerializeObject(state, _Options);
			File.WriteAllText(_File, lines);
			return Observable.Return(Unit.Default);
		}
	}

	public class Test : CustomCreationConverter<MainViewModel>
	{
		public override MainViewModel Create(Type objectType)
		{
			throw new NotImplementedException();
		}
	}

	public class Test2 : CustomCreationConverter<SongViewModel>
	{
		public override SongViewModel Create(Type objectType)
		{
			throw new NotImplementedException();
		}
	}

	public class Test3 : CustomCreationConverter<ISongLoader>
	{
		public override ISongLoader Create(Type objectType)
		{
			throw new NotImplementedException();
		}
	}
}