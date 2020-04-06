using System;
using System.Diagnostics;
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

		public bool RetryUntilSuccess { get; set; }

		static SourceInfoGatherer()
		{
			_Options.Converters.Add(new AspectRatioJsonConverter());
		}

		public Task<AudioInfo> GetAudioInfoAsync(string file, int track = 0)
			=> GetInfoAsync<AudioInfo>('a', file, track);

		public async Task<VolumeInfo> GetAverageVolumeAsync(string file)
		{
			if (!File.Exists(file))
			{
				throw new ArgumentException($"{file} does not exist.", nameof(file));
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

			using var process = ProcessUtils.CreateProcess(ProcessUtils.FFmpeg, args);

			process.WithCleanUp((s, e) =>
			{
				process.Kill();
				process.Dispose();
			}, _ => { });

			var info = new VolumeInfo();
			void OnErrorReceived(object sender, DataReceivedEventArgs args)
			{
				if (args.Data == null)
				{
					return;
				}

				const string LINE_START = "[Parsed_volumedetect_0 @";
				if (!args.Data.StartsWith(LINE_START))
				{
					return;
				}

				var cut = args.Data.Split(']')[1].Trim();
				var kvp = cut.Split(':');
				string key = kvp[0], value = kvp[1];

				Action<VolumeInfo> f = key switch
				{
					"n_samples" => x => x.NSamples = int.Parse(value),
					"mean_volume" => x => x.MeanVolume = VolumeModifer.Parse(value).Decibels!.Value,
					"max_volume" => x => x.MaxVolume = VolumeModifer.Parse(value).Decibels!.Value,
					_ => x => //histogram_#db
					{
						var db = key.Split('_')[1].Split('d')[0];
						x.Histograms[int.Parse(db)] = int.Parse(value);
					}
				};
				f(info);
			}

			process.ErrorDataReceived += OnErrorReceived;
			await process.RunAsync(false).CAF();
			process.ErrorDataReceived -= OnErrorReceived;

			return info;
		}

		public Task<VideoInfo> GetVideoInfoAsync(string file, int track = 0)
			=> GetInfoAsync<VideoInfo>('v', file, track);

		private async Task<T> GetInfoAsync<T>(char stream, string file, int track)
		{
			if (!File.Exists(file))
			{
				throw new ArgumentException($"{file} does not exist.", nameof(file));
			}

			#region Args
			const string ARGS = "-v quiet" +
				" -print_format json" +
				" -show_streams";

			var args = ARGS +
				$" -select_streams {stream}:{track}" +
				$" \"{file}\"";
			#endregion Args

			using var process = ProcessUtils.CreateProcess(ProcessUtils.FFprobe, args);

			process.WithCleanUp((s, e) =>
			{
				process.Kill();
				process.Dispose();
			}, _ => { });

			var sb = new StringBuilder();
			void OnOutputReceived(object sender, DataReceivedEventArgs args)
				=> sb.Append(args.Data);

			process.OutputDataReceived += OnOutputReceived;
			await process.RunAsync(false).CAF();
			process.OutputDataReceived -= OnOutputReceived;

			try
			{
				using var doc = JsonDocument.Parse(sb.ToString());

				var infoJson = doc.RootElement.GetProperty("streams")[0];
				return infoJson.ToObject<T>(_Options);
			}
			catch (Exception e)
			{
				if (RetryUntilSuccess)
				{
					return await GetInfoAsync<T>(stream, file, track).CAF();
				}
				throw new JsonException($"Unable to parse {stream} info for {file}.", e);
			}
		}
	}
}