using System.Text;
using System.Text.Json;

using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor.FFmpeg
{
	public sealed class SourceInfoGatherer : ISourceInfoGatherer
	{
		private static readonly JsonSerializerOptions _Options = new();
		private static readonly char[] _SplitChars = new[] { '_', 'd' };

		public int RetryLimit { get; set; } = 0;

		static SourceInfoGatherer()
		{
			_Options.Converters.Add(new AspectRatioJsonConverter());
		}

		public Task<SourceInfo<AudioInfo>> GetAudioInfoAsync(string file, int track = 0)
			=> GetInfoAsync<AudioInfo>('a', file, track, 0);

		public async Task<SourceInfo<VolumeInfo>> GetAverageVolumeAsync(string file)
		{
			if (!File.Exists(file))
			{
				throw new FileNotFoundException("File does not exist to get average volume.", file);
			}

			#region Args
			const string ARGS = "-vn " +
				"-sn " +
				"-dn ";
			const string OUTPUT_ARGS = " -af \"volumedetect\" " +
				"-f null " +
				"-";

			var args = ARGS +
				$" -i \"{file}\"" +
				OUTPUT_ARGS;
			#endregion Args

			using var process = ProcessUtils.FFmpeg.CreateProcess(args);
			process.WithCleanUp((s, e) =>
			{
				process.Kill();
				process.Dispose();
			}, null);

			var histograms = new Dictionary<int, int>();
			var maxVolume = 0.00;
			var meanVolume = 0.00;
			var nSamples = 0;
			process.ErrorDataReceived += (s, e) =>
			{
				if (e.Data is null)
				{
					return;
				}

				const string LINE_START = "[Parsed_volumedetect_0 @";
				if (!e.Data.StartsWith(LINE_START))
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

			return new SourceInfo<VolumeInfo>(file, new VolumeInfo(
				histograms,
				maxVolume,
				meanVolume,
				nSamples
			));
		}

		public Task<SourceInfo<VideoInfo>> GetVideoInfoAsync(string file, int track = 0)
			=> GetInfoAsync<VideoInfo>('v', file, track, 0);

		private static SourceInfoGatheringException Exception(char stream, string file, Exception inner)
			=> new($"Unable to gather {stream} info for {file}.", inner);

		private async Task<SourceInfo<T>> GetInfoAsync<T>(char stream, string file, int track, int attempt)
		{
			if (!File.Exists(file))
			{
				throw Exception(stream, file, new FileNotFoundException("File does not exist.", file));
			}

			#region Args
			const string ARGS = "-v quiet" +
				" -print_format json" +
				" -show_streams";

			var args = ARGS +
				$" -select_streams {stream}:{track}" +
				$" \"{file}\"";
			#endregion Args

			using var process = ProcessUtils.FFprobe.CreateProcess(args);
			process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
			process.StartInfo.RedirectStandardOutput = true;
			process.WithCleanUp((s, e) =>
			{
				process.Kill();
				process.Dispose();
			}, null);

			await process.RunAsync(OutputMode.Sync).ConfigureAwait(false);
			// Must call WaitForExit otherwise the json may be incomplete
			await process.WaitForExitAsync().ConfigureAwait(false);

			try
			{
				using var doc = await JsonDocument.ParseAsync(process.StandardOutput.BaseStream).ConfigureAwait(false);
				if (!doc.RootElement.TryGetProperty("streams", out var property))
				{
					throw Exception(stream, file, new InvalidFileTypeException("Invalid file type."));
				}
				var info = property[0].ToObject<T>(_Options);
				if (info is null)
				{
					throw Exception(stream, file, new JsonException("Invalid json supplied."));
				}
				return new SourceInfo<T>(file, info);
			}
			catch (JsonException je)
			{
				throw Exception(stream, file, new JsonException("Unable to parse JSON. Possibly attempted to parse before stream was fully written to.", je));
			}
			catch (Exception e) when (!(e is SourceInfoGatheringException))
			{
				if (RetryLimit > attempt)
				{
					return await GetInfoAsync<T>(stream, file, track, attempt + 1).ConfigureAwait(false);
				}
				throw Exception(stream, file, e);
			}
		}
	}
}