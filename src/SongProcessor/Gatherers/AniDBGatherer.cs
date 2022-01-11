using HtmlAgilityPack;

using SongProcessor.Models;

using System.IO.Compression;
using System.Web;

namespace SongProcessor.Gatherers;

public sealed class AniDBGatherer : IAnimeGatherer
{
	private const string URL = "https://anidb.net/anime/";

	private readonly HttpClient _Client;
	public string Name { get; } = "AniDB";

	public AniDBGatherer(HttpClient? client = null)
	{
		_Client = client ?? CreateClient();
	}

	public async Task<AnimeBase> GetAsync(int id, GatherOptions? options = null)
	{
		var response = await _Client.GetAsync(URL + id).ConfigureAwait(false);
		response.ThrowIfInvalidResponse();

		var doc = new HtmlDocument();
		// AniDB uses brotli compression
		using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
		using (var br = new BrotliStream(stream, CompressionMode.Decompress))
		{
			doc.Load(br);
		}
		return Parse(doc.DocumentNode, id, options);
	}

	public override string ToString()
		=> Name;

	async Task<IAnimeBase> IAnimeGatherer.GetAsync(int id, GatherOptions? options)
		=> await GetAsync(id, options).ConfigureAwait(false);

	internal AnimeBase Parse(HtmlNode doc, int id, GatherOptions? options)
	{
		if (doc.Descendants("div").Any(x => x.HasClass("error")))
		{
			this.ThrowUnableToFind(id);
		}

		return new()
		{
			Id = Get(doc, GetANNId, "ANN ID", id),
			Name = Get(doc, GetTitle, "title", id),
			Songs = new(Get(doc, x => GetSongs(x, options), "songs", id)),
			Year = Get(doc, GetYear, "year", id),
		};
	}

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

	private static int GetANNId(HtmlNode doc)
	{
		var a = doc.Descendants("a");
		var annBrand = a.Single(x => x.HasClass("i_resource_ann"));
		var href = annBrand.GetAttributeValue("href", null);
		return int.Parse(href.Split("id=")[1]);
	}

	private static IEnumerable<Song> GetSongs(HtmlNode doc, GatherOptions? options)
	{
		const string SONG = "song";
		const string CREATOR = "creator";
		const string RELTYPE = "reltype";

		var type = default(SongType?);
		var count = 1;
		foreach (var tr in doc.Descendants("tr"))
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
							if (current is not null)
							{
								throw new InvalidOperationException($"Duplicate {@class}.");
							}
							dict[@class] = HttpUtility.HtmlDecode(td.InnerText.Trim());
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

			if (dict.Values.Any(x => x is null)
				|| !type.HasValue
				|| options?.CanBeGathered(type.Value) == false)
			{
				continue;
			}

			yield return new Song
			{
				Type = new(type.Value, count++),
				Name = dict[SONG]!,
				Artist = dict[CREATOR]!,
			};
		}
	}

	private static string GetTitle(HtmlNode doc)
	{
		var div = doc.Descendants("div");
		var data = div.Single(x => x.Id == "tab_1_pane");
		var span = data.Descendants("span");
		var name = span.Single(x => x.GetAttributeValue("itemprop", null) == "name");
		return name.InnerText.Trim();
	}

	private static int GetYear(HtmlNode doc)
	{
		var span = doc.Descendants("span");
		var date = span.Single(x =>
		{
			var itemProp = x.GetAttributeValue("itemprop", null);
			return itemProp is "datePublished" or "startDate";
		});
		var content = date.GetAttributeValue("content", null);
		return DateTime.Parse(content).Year;
	}

	private T Get<T>(HtmlNode doc, Func<HtmlNode, T> func, string item, int id)
	{
		try
		{
			return func(doc);
		}
		catch (Exception e)
		{
			throw new FormatException($"Invalid {item} provided by {Name} for {id}.", e);
		}
	}
}