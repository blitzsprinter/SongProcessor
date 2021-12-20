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

	static SourceInfoGatherer()
	{
		_Options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
		_Options.Converters.Add(new AspectRatioJsonConverter());
		_Options.Converters.Add(new ParseJsonConverter<bool>(bool.Parse));
	}

	public Task<SourceInfo<AudioInfo>> GetAudioInfoAsync(string path, int track = 0)
		=> GetInfoAsync<AudioInfo>('a', path, track);

	public Task<SourceInfo<VideoInfo>> GetVideoInfoAsync(string path, int track = 0)
		=> GetInfoAsync<VideoInfo>('v', path, track);

	public async Task<SourceInfo<VolumeInfo>> GetVolumeInfoAsync(string path, int track = 0)
	{
		if (!File.Exists(path))
		{
			throw FileNotFound('a', path);
		}

		var args =
			"-vn " +
			"-sn " +
			"-dn " +
			$" -i \"{path}\"" +
			$" -map 0:a:{track}" +
			" -af \"volumedetect\" " +
			"-f null " +
			"-";
		using var process = ProcessUtils.FFmpeg.CreateProcess(args);

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

		return new SourceInfo<VolumeInfo>(path, new VolumeInfo(
			histograms,
			maxVolume,
			meanVolume,
			nSamples
		));
	}

	private static SourceInfoGatheringException Exception(char stream, string path, Exception inner)
		=> new($"Unable to gather '{stream}' stream info for {path}.", inner);

	private static SourceInfoGatheringException FileNotFound(char stream, string path)
		=> Exception(stream, path, new FileNotFoundException("File does not exist", path));

	private static async Task<SourceInfo<T>> GetInfoAsync<T>(
		char stream,
		string path,
		int track)
	{
		if (!File.Exists(path))
		{
			throw FileNotFound(stream, path);
		}

		var args =
			"-v quiet" +
			" -print_format json" +
			" -show_streams" +
			$" -select_streams {stream}:{track}" +
			$" \"{path}\"";

		using var process = ProcessUtils.FFprobe.CreateProcess(args);
		process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
		process.StartInfo.RedirectStandardOutput = true;

		await process.RunAsync(OutputMode.Sync).ConfigureAwait(false);
		// Must call WaitForExit otherwise the json may be incomplete
		await process.WaitForExitAsync().ConfigureAwait(false);

		try
		{
			var output = await JsonSerializer.DeserializeAsync<Output<T>>(
				process.StandardOutput.BaseStream,
				_Options
			).ConfigureAwait(false);
			if (output is null || output.Streams.FirstOrDefault() is not T result)
			{
				throw Exception(stream, path, new JsonException("Invalid JSON supplied."));
			}
			return new(path, result);
		}
		catch (Exception e) when (e is not SourceInfoGatheringException)
		{
			throw Exception(stream, path, e);
		}
	}

	private sealed record Output<T>(
		[property: JsonPropertyName("streams")]
			T[] Streams
	);
}