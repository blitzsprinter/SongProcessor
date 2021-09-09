using System.Reactive;
using System.Reactive.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using ReactiveUI;

namespace SongProcessor.UI
{
	public class NewtonsoftJsonSuspensionDriver : ISuspensionDriver
	{
		private readonly JsonSerializerSettings _Options = new()
		{
			ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
			ContractResolver = new WritablePropertiesOnlyResolver(),
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.All,
		};
		private readonly string _Path;
		public bool DeleteOnInvalidState { get; set; }

		public NewtonsoftJsonSuspensionDriver(string path)
		{
			_Path = path;
		}

		public IObservable<Unit> InvalidateState()
		{
			if (DeleteOnInvalidState && File.Exists(_Path))
			{
				File.Delete(_Path);
			}
			return Observable.Return(Unit.Default);
		}

		public IObservable<object> LoadState()
		{
			if (!File.Exists(_Path))
			{
				return Observable.Return(default(object))!;
			}

			var lines = File.ReadAllText(_Path);
			var state = JsonConvert.DeserializeObject<object>(lines, _Options);
			return Observable.Return(state)!;
		}

		public IObservable<Unit> SaveState(object state)
		{
			var lines = JsonConvert.SerializeObject(state, _Options);
			File.WriteAllText(_Path, lines);
			return Observable.Return(Unit.Default);
		}

		private sealed class WritablePropertiesOnlyResolver : DefaultContractResolver
		{
			protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
			{
				var props = base.CreateProperties(type, memberSerialization);
				for (var i = props.Count - 1; i >= 0; --i)
				{
					if (!props[i].Writable)
					{
						props.RemoveAt(i);
					}
				}
				return props;
			}
		}
	}
}