using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AMQSongProcessor.UI
{
	public class ObservableCollectionPlus<T> : ObservableCollection<T>
	{
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(Items)));
			base.OnCollectionChanged(e);
		}
	}
}