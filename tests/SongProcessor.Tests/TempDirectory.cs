using Microsoft.VisualStudio.TestTools.UnitTesting;

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
		Assert.AreEqual(0, Directory.GetFiles(Dir).Length);
	}

	public void Dispose()
	{
		if (Directory.Exists(Dir))
		{
			Directory.Delete(Dir, true);
		}
	}
}
