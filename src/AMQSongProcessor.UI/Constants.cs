using System.Collections.Immutable;

namespace AMQSongProcessor.UI
{
	public static class Constants
	{
		public const int MESSAGE_BOX_HEIGHT = 133;
		public const int MESSAGE_BOX_WIDTH = 278;
		public const string NO = "No";
		public const string YES = "Yes";

		public static readonly ImmutableArray<string> YesNo
			= new[] { YES, NO }.ToImmutableArray();
	}
}