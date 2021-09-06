using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.Converters
{
	public abstract class JsonConverter_TestsBase<TItem, TConverter>
		where TConverter : JsonConverter<TItem>
	{
		public abstract TConverter Converter { get; }
		public abstract string Json { get; }
		public virtual Lazy<JsonSerializerOptions> Options { get; }
		public abstract TItem Value { get; }

		protected JsonConverter_TestsBase()
		{
			Options = new(() =>
			{
				var options = new JsonSerializerOptions()
				{
					IgnoreReadOnlyProperties = true,
				};
				options.Converters.Add(new JsonStringEnumConverter());
				options.Converters.Add(Converter);
				ConfigureOptions(options);
				return options;
			});
		}

		[TestMethod]
		public virtual async Task Deserialize_Test()
		{
			var actual = await DeserializeAsync<Foo>(Json).ConfigureAwait(false);
			AssertEqual(actual.Value);
		}

		[TestMethod]
		public virtual async Task Serialize_Test()
		{
			var actual = await SerializeAsync(new Foo
			{
				Value = Value
			}).ConfigureAwait(false);
			Assert.AreEqual(Json, actual);
		}

		protected virtual void AssertEqual(TItem actual)
			=> Assert.AreEqual(Value, actual);

		protected virtual void ConfigureOptions(JsonSerializerOptions options)
		{
		}

		protected virtual async Task<T> DeserializeAsync<T>(string input)
		{
			using var ms = new MemoryStream();
			using var writer = new StreamWriter(ms);

			await writer.WriteAsync(input).ConfigureAwait(false);
			await writer.FlushAsync().ConfigureAwait(false);
			ms.Seek(0, SeekOrigin.Begin);

			return (await JsonSerializer.DeserializeAsync<T>(ms, Options.Value).ConfigureAwait(false))!;
		}

		protected virtual async Task<string> SerializeAsync<T>(T value)
		{
			using var ms = new MemoryStream();
			using var reader = new StreamReader(ms);

			await JsonSerializer.SerializeAsync(ms, value, Options.Value).ConfigureAwait(false);
			await ms.FlushAsync().ConfigureAwait(false);
			ms.Seek(0, SeekOrigin.Begin);

			return await reader.ReadToEndAsync().ConfigureAwait(false);
		}

		protected sealed class Foo
		{
			public TItem Value { get; set; } = default!;
		}
	}
}