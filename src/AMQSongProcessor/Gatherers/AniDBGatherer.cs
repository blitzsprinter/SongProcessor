using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Models;

using HtmlAgilityPack;

namespace AMQSongProcessor.Gatherers
{
	public sealed class AniDBGatherer : IAnimeGatherer
	{
		private const string URL = "https://anidb.net/anime/";

		private readonly HttpClient _Client;
		public string Name { get; } = "AniDB";

		public AniDBGatherer(HttpClient? client = null)
		{
			_Client = client ?? CreateClient();
		}

		public async Task<IAnimeBase> GetAsync(int id, GatherOptions? options = null)
		{
			var url = URL + id;
			var result = await _Client.GetAsync(url).CAF();
			if (!result.IsSuccessStatusCode)
			{
				throw new HttpRequestException($"{url} threw {result.StatusCode}.");
			}

			var doc = new HtmlDocument();
			//aniDB uses brotli compression
			using (var stream = await result.Content.ReadAsStreamAsync().CAF())
			using (var br = new BrotliStream(stream, CompressionMode.Decompress))
			{
				doc.Load(br);
			}
			if (doc.DocumentNode.Descendants("div").Any(x => x.HasClass("error")))
			{
				throw new HttpRequestException($"{id} does not exist on {Name}.");
			}

			try
			{
				var anime = new AnimeModel
				{
					Id = GetANNId(doc),
					Name = GetTitle(doc),
					Year = GetYear(doc),
				};
				anime.Songs.AddRange(GetSongs(doc, options));
				return anime;
			}
			catch (Exception e)
			{
				throw new FormatException($"Incorrect html for anime from {url}.", e);
			}
		}

		public override string ToString()
			=> Name;

		private static HttpClient CreateClient()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
			client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, default, br");
			client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9"); //Make sure we get English results
			client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
			client.DefaultRequestHeaders.Add("Connection", "keep-alive");
			client.DefaultRequestHeaders.Add("pragma", "no-cache");
			client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36");
			return client;
		}

		private static int GetANNId(HtmlDocument doc)
		{
			try
			{
				var a = doc.DocumentNode.Descendants("a");
				var annBrand = a.Single(x => x.HasClass("i_resource_ann"));
				var href = annBrand.GetAttributeValue("href", null);
				return int.Parse(href.Split("id=")[1]);
			}
			catch (Exception e)
			{
				throw new FormatException("Unable to get ANN id.", e);
			}
		}

		private static IEnumerable<Song> GetSongs(HtmlDocument doc, GatherOptions? options)
		{
			const string SONG = "song";
			const string CREATOR = "creator";
			const string RELTYPE = "reltype";

			var type = default(SongType?);
			var count = 1;
			foreach (var tr in doc.DocumentNode.Descendants("tr"))
			{
				var dict = new Dictionary<string, string?>(2)
				{
					[SONG] = null,
					[CREATOR] = null,
				};

				try
				{
					foreach (var td in tr.Descendants("td"))
					{
						foreach (var @class in td.GetClasses())
						{
							if (dict.TryGetValue(@class, out var current))
							{
								if (current != null)
								{
									throw new InvalidOperationException($"Duplicate {@class}.");
								}
								dict[@class] = td.InnerText.Trim();
							}
							else if (@class == RELTYPE)
							{
								var s = td.InnerText.Split()[0];
								if (Enum.TryParse<SongType>(s, true, out var temp))
								{
									type = temp;
									count = 1;
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					throw new FormatException("Unable to get songs.", e);
				}

				if (dict.Values.Any(x => x == null)
					|| !type.HasValue
					|| options?.CanBeGathered(type.Value) == false)
				{
					continue;
				}

				yield return new Song
				{
					Type = new SongTypeAndPosition(type.Value, count++),
					Name = dict[SONG]!,
					Artist = dict[CREATOR]!,
				};
			}
		}

		private static string GetTitle(HtmlDocument doc)
		{
			try
			{
				var div = doc.DocumentNode.Descendants("div");
				var data = div.Single(x => x.Id == "tab_1_pane");

				var span = data.Descendants("span");
				var name = span.Single(x => x.GetAttributeValue("itemprop", null) == "name");
				return name.InnerText.Trim();
			}
			catch (Exception e)
			{
				throw new FormatException("Unable to get title.", e);
			}
		}

		private static int GetYear(HtmlDocument doc)
		{
			try
			{
				var span = doc.DocumentNode.Descendants("span");
				var date = span.Single(x =>
				{
					var itemProp = x.GetAttributeValue("itemprop", null);
					return itemProp == "datePublished" || itemProp == "startDate";
				});
				var content = date.GetAttributeValue("content", null);
				return DateTime.Parse(content).Year;
			}
			catch (Exception e)
			{
				throw new FormatException("Unable to get year.", e);
			}
		}
	}
}