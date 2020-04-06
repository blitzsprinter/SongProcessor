using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AMQSongProcessor.Utils
{
	public static class Utils
	{
		public static T[] ToArray<T>(this IEnumerable<T> source, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			var array = new T[count];
			var i = 0;
			foreach (var item in source)
			{
				array[i++] = item;
			}
			return array;
		}

		public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable)
		{
			var list = new List<T>();
			await foreach (var value in enumerable)
			{
				list.Add(value);
			}
			return list;
		}

		public static T ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
		{
			var json = element.GetRawText();
			return JsonSerializer.Deserialize<T>(json, options);
		}

		public static T ToObject<T>(this JsonDocument document, JsonSerializerOptions? options = null)
		{
			if (document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}
			return document.RootElement.ToObject<T>(options);
		}
	}
}