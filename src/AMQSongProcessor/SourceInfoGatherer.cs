using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public sealed class SourceInfoGatherer : ISourceInfoGatherer
	{
		private static readonly JsonSerializerOptions _Options = new JsonSerializerOptions();

		static SourceInfoGatherer()
		{
			_Options.Converters.Add(new AspectRatioJsonConverter());
		}

		public Task<AudioInfo> GetAudioInfoAsync(string file, int track = 0)
			=> GetInfoAsync<AudioInfo>('a', file, track);

		public Task<string> GetAverageVolumeAsync(string file)
			=> throw new NotImplementedException();

		public Task<VideoInfo> GetVideoInfoAsync(string file, int track = 0)
			=> GetInfoAsync<VideoInfo>('v', file, track);

		private async Task<T> GetInfoAsync<T>(char stream, string file, int track)
		{
			if (file == null)
			{
				return default;
			}

			#region Args
			const string ARGS = "-v quiet" +
				" -print_format json" +
				" -show_streams";

			var args = ARGS +
				$" -select_streams {stream}:{track}" +
				$" \"{file}\"";
			#endregion Args

			using var process = Utils.CreateProcess(Utils.FFprobe, args);

			var sb = new StringBuilder();
			void OnOutputReceived(object sender, DataReceivedEventArgs args)
				=> sb.Append(args.Data);

			process.OutputDataReceived += OnOutputReceived;
			await process.RunAsync(false).CAF();
			process.OutputDataReceived -= OnOutputReceived;

			using var doc = JsonDocument.Parse(sb.ToString());

			var infoJson = doc.RootElement.GetProperty("streams")[0];
			return infoJson.ToObject<T>(_Options);
		}
	}
}