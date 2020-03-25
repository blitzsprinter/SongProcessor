using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AMQSongProcessor.UI
{
	public sealed class WritablePropertiesOnlyResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var props = base.CreateProperties(type, memberSerialization);
			return props.Where(p => p.Writable).ToList();
		}
	}
}