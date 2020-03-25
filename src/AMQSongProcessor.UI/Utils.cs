using System.Runtime.CompilerServices;

using ReactiveUI;

namespace AMQSongProcessor.UI
{
	public static class Utils
	{
		public static string RaiseAndSetIfChangedAndNotNull<T>(
			this T obj,
			ref string field,
			string value,
			[CallerMemberName] string propertyName = null)
			where T : IReactiveObject
		{
			if (value == null)
			{
				return null;
			}

			return obj.RaiseAndSetIfChanged(ref field, value, propertyName);
		}
	}
}