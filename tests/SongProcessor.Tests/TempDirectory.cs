using FluentAssertions;

namespace SongProcessor.Tests;

public sealed class TempDirectory : IDisposable
{
	public string Dir { get; }
	public Guid Guid { get; }
	public string Parent { get; }

	public TempDirectory() : this(Directory.GetCurrentDirectory())
	{
	}

	public TempDirectory(string parent)
	{
		Parent = parent;
		Guid = Guid.NewGuid();
		Dir = Path.Combine(Parent, "temp", Guid.ToString());

		Directory.CreateDirectory(Dir);
		Directory.GetFiles(Dir).Length.Should().Be(0);
	}

	public void Dispose()
	{
		if (Directory.Exists(Dir))
		{
			Directory.Delete(Dir, true);
		}
	}
}