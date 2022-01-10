using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SongProcessor.Tests.Converters;

public abstract class JsonConverter_TestsBase<TItem, TConverter>
	where TConverter : JsonConverter<TItem>
{
	public abstract TConverter Converter { get; }
	public abstract string Json { get; }
	public abstract TItem Value { get; }

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
		actual.Should().Be(Json);
	}

	protected virtual void AssertEqual(TItem actual)
		=> actual.Should().Be(Value);

	protected virtual JsonSerializerOptions CreateOptions()
	{
		var options = new JsonSerializerOptions()
		{
			IgnoreReadOnlyProperties = true,
		};
		options.Converters.Add(new JsonStringEnumConverter());
		options.Converters.Add(Converter);
		return options;
	}

	protected virtual async Task<T> DeserializeAsync<T>(string input)
	{
		using var ms = new MemoryStream();
		using var writer = new StreamWriter(ms);

		await writer.WriteAsync(input).ConfigureAwait(false);
		await writer.FlushAsync().ConfigureAwait(false);
		ms.Seek(0, SeekOrigin.Begin);

		return (await JsonSerializer.DeserializeAsync<T>(ms, CreateOptions()).ConfigureAwait(false))!;
	}

	protected virtual async Task<string> SerializeAsync<T>(T value)
	{
		using var ms = new MemoryStream();
		using var reader = new StreamReader(ms);

		await JsonSerializer.SerializeAsync(ms, value, CreateOptions()).ConfigureAwait(false);
		await ms.FlushAsync().ConfigureAwait(false);
		ms.Seek(0, SeekOrigin.Begin);

		return await reader.ReadToEndAsync().ConfigureAwait(false);
	}

	protected sealed class Foo
	{
		public TItem Value { get; set; } = default!;
	}
}