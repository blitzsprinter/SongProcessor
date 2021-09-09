
using SongProcessor.Models;

namespace SongProcessor.UI.Models
{
	[Flags]
	public enum StatusModifier
	{
		NotRes720 = -Res720,
		NotRes480 = -Res480,
		NotMp3 = -Mp3,
		NotSubmitted = -Submitted,
		None = 0,
		Submitted = (1 << 0),
		Mp3 = (1 << 1),
		Res480 = (1 << 2),
		Res720 = (1 << 3),
	}

	public static class StatusModifierUtils
	{
		public static Status ToStatus(this StatusModifier modifier)
		{
			var abs = (StatusModifier)Math.Abs((int)modifier);
			return abs switch
			{
				StatusModifier.Submitted => Status.Submitted,
				StatusModifier.Mp3 => Status.Mp3,
				StatusModifier.Res480 => Status.Res480,
				StatusModifier.Res720 => Status.Res720,
				_ => throw new ArgumentOutOfRangeException(nameof(modifier)),
			};
		}
	}
}