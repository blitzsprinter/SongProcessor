using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

using AdvorangesUtils;

using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public static class ANNGatherer
	{
		private const string URL = "https://cdn.animenewsnetwork.com/encyclopedia/api.xml?anime=";
		private static readonly HttpClient _Client = new HttpClient();

		public static async Task<Anime> GatherBarebones(int id)
		{
			var url = URL + id;
			var result = await _Client.GetAsync(url).CAF();
			if (!result.IsSuccessStatusCode)
			{
				throw new HttpRequestException($"{url} threw {result.StatusCode}.");
			}

			var stream = await result.Content.ReadAsStreamAsync().CAF();
			var doc = XElement.Load(stream);

			var title = "";
			var vintage = long.MaxValue;
			foreach (var info in doc.Descendants("info"))
			{
				switch (info.Attribute("type").Value.ToLower())
				{
					case "main title":
						title = info.Value;
						break;

					case "vintage":
						DateTime dt;
						try
						{
							dt = DateTime.Parse(info.Value.Split(' ')[0]);
						}
						catch (FormatException fe)
						{
							throw new FormatException($"Invalid date format provided by ANN for {id}", fe);
						}
						vintage = Math.Min(vintage, dt.Ticks);
						break;
				}
			}

			return new Anime
			{
				Name = title,
				Year = new DateTime(vintage).Year,
			};
		}
	}
}