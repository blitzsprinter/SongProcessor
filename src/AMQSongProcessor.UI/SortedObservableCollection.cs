using System;
using System.Collections;
using System.Collections.Generic;

namespace AMQSongProcessor.UI
{
	public class SortedObservableCollection<T> : ObservableCollectionPlus<T>
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
			=> throw new InvalidOperationException($"Items cannot be moved in a {nameof(SortedObservableCollection<T>)}.");

		protected override void SetItem(int index, T item)
		{
			index = GetSortedIndex(item);
			base.SetItem(index, item);
		}
	}
}