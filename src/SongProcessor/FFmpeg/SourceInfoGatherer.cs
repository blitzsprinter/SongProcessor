using SongProcessor.Converters;
using SongProcessor.Models;
using SongProcessor.Utils;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SongProcessor.FFmpeg;

public sealed class SourceInfoGatherer : ISourceInfoGatherer
{
	private static readonly JsonSerializerOptions _Options = new();
	private static readonly char[] _SplitChars = new[] { '_', 'd' };
	private static readonly Dictionary<string, string> _VolumeArgs = new()
	{
		["vn"] = "",
		["sn"] = "",
		["dn"] = "",
		["f"] = "null",
	};
	private static readonly Dictionary<string, string> _VolumeAudioFilters = new()
	{
		["volumedetect"] = "",
	};

	static SourceInfoGatherer()
	{
		_Options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
		_Options.Converters.Add(new AspectRatioJsonConverter());
		_Options.Converters.Add(new ParseJsonConverter<bool>(bool.Parse));
	}

	public Task<AudioInfo> GetAudioInfoAsync(string file, int track = 0)
		=> GetInfoAsync<AudioInfo>('a', file, track);

	public Task<VideoInfo> GetVideoInfoAsync(string file, int track = 0)
		=> GetInfoAsync<VideoInfo>('v', file, track);

	public async Task<VolumeInfo> GetVolumeInfoAsync(string file, int track = 0)
	{
		if (!File.Exists(file))
		{
			throw FileNotFound(file, 'a');
		}

		var args = new FFmpegArgs(
			Inputs: new FFmpegInput[]
			{
				new(file, null),
			},
			Mapping: new[]
			{
				$"0:a:{track}",
			},
			Args: _VolumeArgs,
			AudioFilters: _VolumeAudioFilters,
			VideoFilters: null,
			OutputFile: "-"
		);
		using var process = ProcessUtils.FFmpeg.CreateProcess(args.ToString());

		var histograms = new Dictionary<int, int>();
		var maxVolume = 0.00;
		var meanVolume = 0.00;
		var nSamples = 0;
		process.ErrorDataReceived += (s, e) =>
		{
			const string LINE_START = "[Parsed_volumedetect_0 @";
			if (e.Data?.StartsWith(LINE_START) != true)
			{
				return;
			}

			var cut = e.Data.Split(']')[1].Trim();
			var kvp = cut.Split(':');
			string key = kvp[0], value = kvp[1];

			switch (key)
			{
				case "n_samples":
					nSamples = int.Parse(value);
					break;

				case "mean_volume":
					meanVolume = VolumeModifer.Parse(value).Value;
					break;

				case "max_volume":
					maxVolume = VolumeModifer.Parse(value).Value;
					break;

				default: // histogram_#db
					var db = int.Parse(key.Split(_SplitChars)[1]);
					histograms[db] = int.Parse(value);
					break;
			}
		};
		await process.RunAsync(OutputMode.Async).ConfigureAwait(false);

		return new(
			File: file,
			Histograms: histograms,
			MaxVolume: maxVolume,
			MeanVolume: meanVolume,
			NSamples: nSamples
		);
	}

	private static SourceInfoGatheringException FileNotFound(string file, char stream)
		=> new(file, stream, new FileNotFoundException("File does not exist", file));

	private static async Task<T> GetInfoAsync<T>(
		char stream,
		string file,
		int track)
		where T : SourceInfo
	{
		if (!File.Exists(file))
		{
			throw FileNotFound(file, stream);
		}

		var args = new FFmpegArgs(
			Inputs: Array.Empty<FFmpegInput>(),
			Mapping: Array.Empty<string>(),
			Args: new Dictionary<string, string>
			{
				["v"] = "quiet",
				["print_format"] = "json",
				["show_streams"] = "",
				["select_streams"] = $"{stream}:{track}",
			},
			AudioFilters: null,
			VideoFilters: null,
			OutputFile: file
		);
		using var process = ProcessUtils.FFprobe.CreateProcess(args.ToString());
		process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
		process.StartInfo.RedirectStandardOutput = true;

		await process.RunAsync(OutputMode.Sync).ConfigureAwait(false);
		// Must call WaitForExit otherwise the json may be incomplete
		await process.WaitForExitAsync().ConfigureAwait(false);

		T info;
		try
		{
			var output = await JsonSerializer.DeserializeAsync<Output<T>>(
				process.StandardOutput.BaseStream,
				_Options
			).ConfigureAwait(false);
			info = output!.Streams.Single();
		}
		catch (Exception e)
		{
			throw new SourceInfoGatheringException(file, stream, e);
		}

		return info with
		{
			File = file
		};
	}

	private sealed record Output<T>(
		[property: JsonPropertyName("streams")]
		T[] Streams
	);
}