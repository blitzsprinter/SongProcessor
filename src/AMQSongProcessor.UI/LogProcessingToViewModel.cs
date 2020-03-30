using System;

namespace AMQSongProcessor.UI
{
	public sealed class LogProcessingToViewModel : IProgress<ProcessingData>
	{
		private readonly Action<ProcessingData> _Setter;

		public LogProcessingToViewModel(Action<ProcessingData> setter)
		{
			_Setter = setter;
		}

		public void Report(ProcessingData value)
			=> _Setter(value);
	}
}