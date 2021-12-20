using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Utils;

namespace SongProcessor.Tests.Utils;

[TestClass]
public sealed class FileUtils_Tests
{
	public static string Dir { get; } = Path.Combine("C:", "joe", "mama");
	public static string Name { get; } = "dn.txt";

	[TestMethod]
	public void EnsureAbsolutePathNotQualified_Test()
	{
		var path = Path.Combine("joe", Name);
		Assert.AreEqual(Path.Combine(Dir, path), FileUtils.EnsureAbsolutePath(Dir, path));
	}

	[TestMethod]
	public void EnsureAbsolutePathPathNull_Test()
		=> Assert.IsNull(FileUtils.EnsureAbsolutePath("C:", null));

	[TestMethod]
	public void EnsureAbsolutePathQualified_Test()
	{
		var path = Path.Combine("C:", Name);
		Assert.AreEqual(path, FileUtils.EnsureAbsolutePath(Dir, path));
	}

	[TestMethod]
	public void GetRelativeOrAbsolutePathAbsolute_Test()
	{
		var path = Path.Combine("C:", Name);
		Assert.AreEqual(path, FileUtils.GetRelativeOrAbsolutePath(Dir, path));
	}

	[TestMethod]
	public void GetRelativeOrAbsolutePathNested_Test()
	{
		var nestedPath = Path.Combine("nested", Name);
		var path = Path.Combine(Dir, nestedPath);
		Assert.AreEqual(nestedPath, FileUtils.GetRelativeOrAbsolutePath(Dir, path));
	}

	[TestMethod]
	public void GetRelativeOrAbsolutePathNull_Test()
		=> Assert.IsNull(FileUtils.GetRelativeOrAbsolutePath(Dir, null));

	[TestMethod]
	public void GetRelativeOrAbsolutePathRelative_Test()
	{
		var path = Path.Combine(Dir, Name);
		Assert.AreEqual(Path.GetFileName(path), FileUtils.GetRelativeOrAbsolutePath(Dir, path));
	}

	[TestMethod]
	public void NextAvailableFileNameAvailable_Test()
	{
		using var temp = new TempDirectory();

		var path = Path.Combine(temp.Dir, Name);
		Assert.AreEqual(path, FileUtils.NextAvailableFilename(path));
	}

	[TestMethod]
	public void NextAvailableFileNameWithExtension_Test()
		=> NextAvailableFileName_Test(".txt");

	[TestMethod]
	public void NextAvailableFileNameWithoutExtension_Test()
		=> NextAvailableFileName_Test(null);

	[TestMethod]
	public void SanitizePathInvalidCharacter_Test()
	{
		var name = Path.GetInvalidFileNameChars()[0] + Name;
		Assert.AreEqual(Name, FileUtils.SanitizePath(name));
	}

	[TestMethod]
	public void SanitizePathNoInvalidCharacters_Test()
		=> Assert.AreEqual(Name, FileUtils.SanitizePath(Name));

	private static void NextAvailableFileName_Test(string? extension)
	{
		const string NAME = "dn";

		using var temp = new TempDirectory();

		var file = Path.Combine(temp.Dir, NAME + extension);
		File.Create(file).Dispose();

		for (var i = 0; i < 5; ++i)
		{
			File.Create(FileUtils.NextAvailableFilename(file)).Dispose();
		}

		var expected = new HashSet<string>
			{
				file,
				Path.Combine(temp.Dir, $"{NAME}_(1){extension}"),
				Path.Combine(temp.Dir, $"{NAME}_(2){extension}"),
				Path.Combine(temp.Dir, $"{NAME}_(3){extension}"),
				Path.Combine(temp.Dir, $"{NAME}_(4){extension}"),
				Path.Combine(temp.Dir, $"{NAME}_(5){extension}"),
			};
		foreach (var item in Directory.EnumerateFiles(temp.Dir))
		{
			Assert.IsTrue(expected.Contains(item));
		}
	}
}