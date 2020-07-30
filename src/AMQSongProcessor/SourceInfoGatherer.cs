using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor
{
	public sealed class SourceInfoGatherer : ISourceInfoGatherer
	{
		private static readonly JsonSerializerOptions _Options = new JsonSerializerOptions();
		private static readonly char[] _SplitChars = new[] { '_', 'd' };

		public int RetryLimit { get; set; } = 0;

		static SourceInfoGatherer()
		{
			_Options.Converters.Add(new AspectRatioJsonConverter());
		}

		public Task<AudioInfo> GetAudioInfoAsync(string file, int track = 0)
			=> GetInfoAsync<AudioInfo>('a', file, track, 0);

		public async Task<VolumeInfo> GetAverageVolumeAsync(string file)
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
			}, _ => { });

			var info = new VolumeInfo();
			process.ErrorDataReceived += (s, e) =>
			{
				if (e.Data == null)
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

				Action<VolumeInfo> f = key switch
				{
					"n_samples" => x => x.NSamples = int.Parse(value),
					"mean_volume" => x => x.MeanVolume = VolumeModifer.Parse(value).Decibels!.Value,
					"max_volume" => x => x.MaxVolume = VolumeModifer.Parse(value).Decibels!.Value,
					_ => x => //histogram_#db
					{
						var db = int.Parse(key.Split(_SplitChars)[1]);
						x.Histograms[db] = int.Parse(value);
					}
				};
				f(info);
			};
			await process.RunAsync(false).CAF();

			return info;
		}

		public Task<VideoInfo> GetVideoInfoAsync(string file, int track = 0)
			=> GetInfoAsync<VideoInfo>('v', file, track, 0);

		private async Task<T> GetInfoAsync<T>(char stream, string file, int track, int attempt)
		{
			if (!File.Exists(file))
			{
				throw new FileNotFoundException($"File does not exist to gather {stream} info.", file);
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
			process.WithCleanUp((s, e) =>
			{
				process.Kill();
				process.Dispose();
			}, _ => { });

			var sb = new StringBuilder();
			process.OutputDataReceived += (s, e) => sb.Append(e.Data);
			await process.RunAsync(false).CAF();

			try
			{
				using var doc = JsonDocument.Parse(sb.ToString());

				var infoJson = doc.RootElement.GetProperty("streams")[0];
				return infoJson.ToObject<T>(_Options);
			}
			catch (KeyNotFoundException knfe) when (sb.Length == 2)
			{
				throw new InvalidFileTypeException($"Invalid file for {stream} info gathering.", file, knfe);
			}
			catch (Exception e) when (!(e is JsonException || e is InvalidFileTypeException))
			{
				if (RetryLimit > attempt)
				{
					return await GetInfoAsync<T>(stream, file, track, attempt + 1).CAF();
				}
				throw new JsonException($"Unable to parse {stream} info for {file}.", e);
			}
		}
	}
}