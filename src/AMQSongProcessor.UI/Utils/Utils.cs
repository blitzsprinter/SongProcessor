using System.Collections.Generic;
using System.Runtime.CompilerServices;

using AMQSongProcessor.UI.ViewModels;

using ReactiveUI;

namespace AMQSongProcessor.UI.Utils
{
	public static class Utils
	{
		public static TRet RaiseAndSetIfChangedSelf<TObj, TRet>(
			this TObj reactiveObject,
			ref TRet backingField,
			TRet newValue,
			[CallerMemberName] string propertyName = "")
			where TObj : IBindableToSelf
		{
			var same = EqualityComparer<TRet>.Default.Equals(backingField, newValue);
			var result = reactiveObject.RaiseAndSetIfChanged(ref backingField, newValue, propertyName);
			if (!same)
			{
				reactiveObject.RaisePropertyChanged(nameof(IBindableToSelf.Self));
			}
			return result;
		}
	}
}