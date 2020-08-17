using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using AdvorangesUtils;

namespace AMQSongProcessor.Utils
{
	public static class FileUtils
	{
		private const string NUMBER_PATTERN = "_({0})";

		private static readonly HashSet<char> InvalidChars =
			new HashSet<char>(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()));

		private static readonly bool IsWindows =
			Environment.OSVersion.Platform.ToString().CaseInsContains("win");

		public static string? EnsureAbsolutePath(string? directory, string? path)
		{
			if (path == null)
			{
				return null;
			}
			else if (Path.IsPathFullyQualified(path))
			{
				return path;
			}

			if (directory == null)
			{
				return null;
			}
			return Path.Combine(directory, path);
		}

		public static string NextAvailableFilename(string path)
		{
			static string GetNextFilename(string pattern)
			{
				var tmp = string.Format(pattern, 1);
				if (tmp == pattern)
				{
					throw new ArgumentException("The pattern must include an index place-holder", nameof(pattern));
				}

				if (!File.Exists(tmp))
				{
					return tmp; // short-circuit if no matches
				}

				int min = 1, max = 2; // min is inclusive, max is exclusive/untested
				while (File.Exists(string.Format(pattern, max)))
				{
					min = max;
					max *= 2;
				}

				while (max != min + 1)
				{
					var pivot = (max + min) / 2;
					if (File.Exists(string.Format(pattern, pivot)))
					{
						min = pivot;
					}
					else
					{
						max = pivot;
					}
				}
				return string.Format(pattern, max);
			}

			// Short-cut if already available
			if (!File.Exists(path))
			{
				return path;
			}

			// If path has extension then insert the number pattern just before the extension and return next filename
			if (Path.HasExtension(path))
			{
				var extStart = path.LastIndexOf(Path.GetExtension(path));
				return GetNextFilename(path.Insert(extStart, NUMBER_PATTERN));
			}

			// Otherwise just append the pattern to the path and return next filename
			return GetNextFilename(path + NUMBER_PATTERN);
		}

		public static bool PathEquals(this string? path1, string? path2)
		{
			//Use CaseInsEquals b/c Windows paths are case insensitive
			if (IsWindows)
			{
				return path1.CaseInsEquals(path2);
			}
			return path1 == path2;
		}

		public static string RemoveInvalidPathChars(string input)
		{
			var sb = new StringBuilder();
			foreach (var c in input)
			{
				if (!InvalidChars.Contains(c))
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}

		public static string? StoreRelativeOrAbsolute(string dir, string? path)
		{
			//If the directory matches the info directory just return the file name
			//Otherwise return the absolute path
			var sourceDir = Path.GetDirectoryName(path);
			return dir.PathEquals(sourceDir) ? Path.GetFileName(path) : path;
		}
	}
}