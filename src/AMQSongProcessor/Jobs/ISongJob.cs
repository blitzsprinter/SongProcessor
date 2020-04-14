using System.Threading;
using System.Threading.Tasks;

namespace AMQSongProcessor.Jobs
{
	public interface ISongJob
	{
		Task<int> ProcessAsync(CancellationToken? token = null);
	}
}