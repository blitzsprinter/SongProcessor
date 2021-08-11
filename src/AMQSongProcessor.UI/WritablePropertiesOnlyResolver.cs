using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AMQSongProcessor.UI
{
	public sealed class WritablePropertiesOnlyResolver : DefaultContractResolver
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