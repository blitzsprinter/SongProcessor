using AMQSongProcessor.Utils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.Utils
{
	[TestClass]
	public sealed class FileUtils_Tests
	{
		private readonly string Dir = Path.Combine("C:", "joe", "mama");
		private readonly string Name = "dn.txt";
		private static string TempPath
			=> Path.Combine(Directory.GetCurrentDirectory(), "temp", Guid.NewGuid().ToString());

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
			var path = Path.Combine(TempPath, Name);
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

			var temp = TempPath;
			var file = Path.Combine(temp, NAME + extension);
			Directory.CreateDirectory(temp);
			File.Create(file);

			for (var i = 0; i < 5; ++i)
			{
				File.Create(FileUtils.NextAvailableFilename(file));
			}

			var expected = new HashSet<string>
			{
				file,
				Path.Combine(temp, $"{NAME}_(1){extension}"),
				Path.Combine(temp, $"{NAME}_(2){extension}"),
				Path.Combine(temp, $"{NAME}_(3){extension}"),
				Path.Combine(temp, $"{NAME}_(4){extension}"),
				Path.Combine(temp, $"{NAME}_(5){extension}"),
			};
			foreach (var item in Directory.EnumerateFiles(temp))
			{
				Assert.IsTrue(expected.Contains(item));
			}
		}
	}
}