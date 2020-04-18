using System;

namespace AMQSongProcessor
{
	internal sealed class LogWarningsToConsole : IProgress<string>
	{
		public void Report(string value)
			=> Console.WriteLine(value);
	}
}