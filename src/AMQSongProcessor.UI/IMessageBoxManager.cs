using System.Collections.Generic;
using System.Threading.Tasks;

namespace AMQSongProcessor.UI
{
	public interface IMessageBoxManager
	{
		Task<T> ShowAsync<T>(string text, string title, IEnumerable<T>? options);
	}
}