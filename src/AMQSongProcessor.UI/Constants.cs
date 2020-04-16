using System.Collections.Immutable;

namespace AMQSongProcessor.UI
{
	public static class Constants
	{
		public const string NO = "No";
		public const string YES = "Yes";

		public static readonly ImmutableArray<string> YesNo
			= new[] { YES, NO }.ToImmutableArray();
	}
}