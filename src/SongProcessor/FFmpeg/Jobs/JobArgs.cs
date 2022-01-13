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
	public override string ToString()
	{
		var sb = new StringBuilder();

		AppendInputs(sb, Inputs);
		AppendMapping(sb, Mapping);
		AppendArgs(sb, QualityArgs);
		AppendFilter(sb, VideoFilters, 'v');
		AppendFilter(sb, AudioFilters, 'a');
		AppendFile(sb, OutputFile);

		return sb.ToString();
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
		sb.AppendJoin(' ', args.Select(x => JoinKvp($"-{x.Key}", x.Value, " ")));
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
		sb.AppendJoin(',', filters.Select(x => JoinKvp(x.Key, x.Value, "=")));
		sb.Append('\"');
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

	private static void AppendMapping(StringBuilder sb, IReadOnlyList<string> mapping)
	{
		foreach (var item in mapping)
		{
			sb.Append(" -map ");
			sb.Append(item);
		}
	}

	private static string JoinKvp(string key, string value, string joiner)
		=> string.IsNullOrWhiteSpace(value) ? key : key + joiner + value;
}