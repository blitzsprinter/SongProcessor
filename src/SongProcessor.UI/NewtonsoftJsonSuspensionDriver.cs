using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using ReactiveUI;

using System.Reactive;
using System.Reactive.Linq;

namespace SongProcessor.UI;

public class NewtonsoftJsonSuspensionDriver : ISuspensionDriver
{
	private readonly string _File;
	private readonly JsonSerializerSettings _Options = new()
	{
		ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
		ContractResolver = new WritablePropertiesOnlyResolver(),
		Formatting = Formatting.Indented,
		TypeNameHandling = TypeNameHandling.All,
	};
	public bool DeleteOnInvalidState { get; set; }

	public NewtonsoftJsonSuspensionDriver(string file)
	{
		_File = file;
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