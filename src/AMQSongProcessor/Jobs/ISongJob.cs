using System.Threading;
using System.Threading.Tasks;

using AMQSongProcessor.Models;

namespace AMQSongProcessor.Jobs
{
	public interface ISongJob
	{
		Song Song { get; }

		Task<int> ProcessAsync(CancellationToken? token = null);
	}
}