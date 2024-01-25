#define USE_NAV_STACK_FIX

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using ReactiveUI;

using SongProcessor.UI.ViewModels;

using System.Reactive;
using System.Reactive.Linq;

namespace SongProcessor.UI;

public class NewtonsoftJsonSuspensionDriver(string Path) : ISuspensionDriver
{
	private readonly JsonSerializerSettings _Options = new()
	{
		ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
		ContractResolver = new WritablePropertiesOnlyResolver(),
		Formatting = Formatting.Indented,
		ObjectCreationHandling = ObjectCreationHandling.Replace,
		SerializationBinder = new NamespaceSerializationBinder(),
		TypeNameHandling = TypeNameHandling.Auto,
	};
	public bool DeleteOnInvalidState { get; set; }

	public IObservable<Unit> InvalidateState()
	{
		if (DeleteOnInvalidState && File.Exists(Path))
		{
			File.Delete(Path);
		}
		return Observable.Return(Unit.Default);
	}

	public IObservable<object> LoadState()
	{
		// ReactiveUI relies on this method throwing an exception
		// to determine if CreateNewAppState should be called
		var lines = File.ReadAllText(Path);
		var state = JsonConvert.DeserializeObject<MainViewModel>(lines, _Options);
		return Observable.Return(state)!;
	}

	public IObservable<Unit> SaveState(object state)
	{
		var lines = JsonConvert.SerializeObject(state, _Options);
		File.WriteAllText(Path, lines);
		return Observable.Return(Unit.Default);
	}

	private sealed class NamespaceSerializationBinder : DefaultSerializationBinder
	{
		const string MyNamespace = nameof(SongProcessor);

		public override Type BindToType(string? assemblyName, string typeName)
		{
			if (!typeName.StartsWith(MyNamespace, StringComparison.OrdinalIgnoreCase))
			{
				throw new JsonSerializationException($"Request type {typeName} not supported.");
			}
			return base.BindToType(assemblyName, typeName);
		}
	}

#if USE_NAV_STACK_FIX

	private sealed class NavigationStackValueProvider(IValueProvider Original) : IValueProvider
	{
		public object? GetValue(object target)
			=> Original.GetValue(target);

		public void SetValue(object target, object? value)
		{
			var castedTarget = (RoutingState)target!;
			var castedValue = (IEnumerable<IRoutableViewModel>)value!;

			castedTarget.NavigationStack.Clear();
			foreach (var viewModel in castedValue)
			{
				castedTarget.NavigationStack.Add(viewModel);
			}
		}
	}

#endif

	private sealed class WritablePropertiesOnlyResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var props = base.CreateProperties(type, memberSerialization);
			for (var i = props.Count - 1; i >= 0; --i)
			{
				var prop = props[i];

#if USE_NAV_STACK_FIX
				if (prop.DeclaringType == typeof(RoutingState)
					&& prop.PropertyName == nameof(RoutingState.NavigationStack))
				{
					prop.Ignored = false;
					prop.Writable = true;
					prop.ValueProvider = new NavigationStackValueProvider(prop.ValueProvider!);
				}
				else
#endif
				if (!prop.Writable)
				{
					props.RemoveAt(i);
				}
			}
			return props;
		}
	}
}