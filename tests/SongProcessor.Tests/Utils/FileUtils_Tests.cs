using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Utils;

namespace SongProcessor.Tests.Utils;

[TestClass]
public sealed class FileUtils_Tests
{
	public static string Dir { get; }
	public static string Name { get; } = "dn.txt";
	public static string Root { get; }

	static FileUtils_Tests()
	{
		Root = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
		Dir = Path.Combine(Root, "joe", "mama");
	}

	[TestMethod]
	public void EnsureAbsolutePathNotQualified_Test()
	{
		var path = Path.Combine("joe", Name);
		FileUtils.EnsureAbsoluteFile(Dir, path).Should().Be(Path.Combine(Dir, path));
	}

	[TestMethod]
	public void EnsureAbsolutePathPathNull_Test()
		=> FileUtils.EnsureAbsoluteFile(Root, null).Should().BeNull();

	[TestMethod]
	public void EnsureAbsolutePathQualified_Test()
	{
		var path = Path.Combine(Root, Name);
		FileUtils.EnsureAbsoluteFile(Dir, path).Should().Be(path);
	}

	[TestMethod]
	public void GetRelativeOrAbsolutePathAbsolute_Test()
	{
		var path = Path.Combine(Root, Name);
		FileUtils.GetRelativeOrAbsoluteFile(Dir, path).Should().Be(path);
	}

	[TestMethod]
	public void GetRelativeOrAbsolutePathNested_Test()
	{
		var nestedPath = Path.Combine("nested", Name);
		var path = Path.Combine(Dir, nestedPath);
		FileUtils.GetRelativeOrAbsoluteFile(Dir, path).Should().Be(nestedPath);
	}

	[TestMethod]
	public void GetRelativeOrAbsolutePathNull_Test()
		=> FileUtils.GetRelativeOrAbsoluteFile(Dir, null).Should().BeNull();

	[TestMethod]
	public void GetRelativeOrAbsolutePathRelative_Test()
	{
		var path = Path.Combine(Dir, Name);
		FileUtils.GetRelativeOrAbsoluteFile(Dir, path).Should().Be(Path.GetFileName(path));
	}

	[TestMethod]
	public void NextAvailableFileNameAvailable_Test()
	{
		using var temp = new TempDirectory();
		var path = Path.Combine(temp.Dir, Name);
		FileUtils.NextAvailableFile(path).Should().Be(path);
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
		FileUtils.SanitizePath(name).Should().Be(Name);
	}

	[TestMethod]
	public void SanitizePathNoInvalidCharacters_Test()
		=> FileUtils.SanitizePath(Name).Should().Be(Name);

	private static void NextAvailableFileName_Test(string? extension)
	{
		const string NAME = "dn";

		using var temp = new TempDirectory();
		var file = Path.Combine(temp.Dir, NAME + extension);
		File.Create(file).Dispose();

		var expected = new HashSet<string> { file };
		for (var i = 0; i < 5; ++i)
		{
			File.Create(FileUtils.NextAvailableFile(file)).Dispose();
			expected.Add(Path.Combine(temp.Dir, $"{NAME} ({i + 1}){extension}"));
		}

		Directory.EnumerateFiles(temp.Dir).Should().Contain(expected);
	}
}