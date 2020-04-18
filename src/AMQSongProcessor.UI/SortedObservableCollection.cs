using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AMQSongProcessor.UI
{
	public class SortedObservableCollection<T> : ObservableCollection<T>
	{
		public IComparer<T> Comparer { get; }

		public SortedObservableCollection(IComparer<T> comparer)
		{
			Comparer = comparer;
		}

		protected int GetSortedIndex(T item)
		{
			lock (((ICollection)this).SyncRoot)
			{
				var i = 0;
				for (; i < Items.Count; ++i)
				{
					if (Comparer.Compare(item, Items[i]) < 1)
					{
						break;
					}
				}
				return i;
			}
		}

		protected override void InsertItem(int index, T item)
		{
			index = GetSortedIndex(item);
			base.InsertItem(index, item);
		}

		protected override void MoveItem(int oldIndex, int newIndex)
		{
		}

		protected override void SetItem(int index, T item)
		{
			index = GetSortedIndex(item);
			base.SetItem(index, item);
		}
	}
}