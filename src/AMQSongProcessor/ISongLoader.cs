using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public interface ISongLoader
	{
		string Extension { get; set; }
		bool RemoveIgnoredSongs { get; set; }

		IAsyncEnumerable<Anime> LoadAsync(string dir);

		Task<Anime> LoadFromANNAsync(int id);

		Task SaveAsync(Anime anime);
	}
}