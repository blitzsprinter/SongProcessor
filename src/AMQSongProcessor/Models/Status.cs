using System;

namespace AMQSongProcessor.Models
{
	[Flags]
	public enum Status : uint
	{
		NotSubmitted = 0,
		None = (1U << 1),
		Mp3 = (1U << 2),
		Res480 = (1U << 3),
		Res720 = (1U << 4),
	}
}