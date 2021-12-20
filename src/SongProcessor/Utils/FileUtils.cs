using System.Text;

namespace SongProcessor.Utils;

public static class FileUtils
{
	private const string NUMBER_PATTERN = "_({0})";
	private static readonly HashSet<char> InvalidChars
		= new(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()));

	public static string? EnsureAbsolutePath(string dir, string? path)
	{
		if (path is null)
		{
			return null;
		}

		return Path.IsPathFullyQualified(path) ? path : Path.Combine(dir, path);
	}

	public static string? GetRelativeOrAbsolutePath(string dir, string? path)
	{
		if (path is null)
		{
			return null;
		}

		// Windows paths are case insensitive
		var comparison = OperatingSystem.IsWindows()
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.CurrentCulture;

		// If the directory contains the info directory just return the nested file path
		// Otherwise return the absolute path
		return path.StartsWith(dir, comparison) ? path[(dir.Length + 1)..] : path;
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

		// If path has extension then insert the number pattern just before the extension
		// and return next filename
		var pattern = Path.HasExtension(path)
			? path.Insert(path.LastIndexOf(Path.GetExtension(path)), NUMBER_PATTERN)
			: path + NUMBER_PATTERN;

		// Otherwise just append the pattern to the path and return next filename
		return GetNextFilename(pattern);
	}

	public static string SanitizePath(string path)
	{
		var sb = new StringBuilder();
		foreach (var c in path)
		{
			if (!InvalidChars.Contains(c))
			{
				sb.Append(c);
			}
		}
		return sb.ToString();
	}
}