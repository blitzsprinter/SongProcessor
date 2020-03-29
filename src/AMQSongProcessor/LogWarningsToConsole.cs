using System;

namespace AMQSongProcessor
{
	public sealed class LogWarningsToConsole : IProgress<string>
	{
		public void Report(string value)
			=> Console.WriteLine(value);
	}
}