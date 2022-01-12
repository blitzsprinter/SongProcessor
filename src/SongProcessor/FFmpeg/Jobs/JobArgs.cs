using System.Text;

namespace SongProcessor.FFmpeg.Jobs;

public record JobArgs(
	IReadOnlyList<JobInput> Inputs,
	IReadOnlyList<string> Mapping,
	IReadOnlyDictionary<string, string> QualityArgs,
	IReadOnlyDictionary<string, string>? AudioFilters,
	IReadOnlyDictionary<string, string>? VideoFilters,
	string OutputFile
)
{
	public IEnumerable<string> GetValues(string key)
	{
		foreach (var input in Inputs)
		{
			if (input.Args?.TryGetValue(key, out var iTemp) == true)
			{
				yield return iTemp;
			}
		}
		if (QualityArgs.TryGetValue(key, out var qTemp))
		{
			yield return qTemp;
		}
		if (AudioFilters?.TryGetValue(key, out var aTemp) == true)
		{
			yield return aTemp;
		}
		if (VideoFilters?.TryGetValue(key, out var vTemp) == true)
		{
			yield return vTemp;
		}
	}

	public override string ToString()
	{
		var sb = new StringBuilder();

		AppendInputs(sb, Inputs);
		AppendArgs(sb, QualityArgs);
		AppendFilter(sb, VideoFilters, 'v');
		AppendFilter(sb, AudioFilters, 'a');
		AppendFile(sb, OutputFile);

		return sb.ToString();
	}

	private static void AppendInputs(StringBuilder sb, IReadOnlyList<JobInput> inputs)
	{
		foreach (var input in inputs)
		{
			AppendArgs(sb, input.Args);
			sb.Append(" -i");
			AppendFile(sb, input.File);
		}
	}

	private static void AppendArgs(
		StringBuilder sb,
		IReadOnlyDictionary<string, string>? args)
	{
		if (args is null || args.Count == 0)
		{
			return;
		}

		sb.Append(' ');
		sb.AppendJoin(' ', args.Select(x => $"-{x.Key} {x.Value}"));
	}

	private static void AppendFile(StringBuilder sb, string outputFile)
		=> sb.Append(" \"").Append(outputFile).Append('\"');

	private static void AppendFilter(
		StringBuilder sb,
		IReadOnlyDictionary<string, string>? filters,
		char channel)
	{
		if (filters is null || filters.Count == 0)
		{
			return;
		}

		sb.Append(" -filter:");
		sb.Append(channel);
		sb.Append(" \"");
		sb.AppendJoin(',', filters.Select(x => $"{x.Key}={x.Value}"));
		sb.Append('\"');
	}
}