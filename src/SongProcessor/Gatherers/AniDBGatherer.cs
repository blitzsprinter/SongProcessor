using HtmlAgilityPack;

using SongProcessor.Models;

using System.IO.Compression;
using System.Web;

namespace SongProcessor.Gatherers;

public sealed class AniDBGatherer : IAnimeGatherer
{
	private const string CREATOR = "creator";
	private const string RELTYPE = "reltype";
	private const string SONG = "song";
	private const string URL = "https://anidb.net/anime/";

	private static readonly HashSet<string> SongProperties = new() { SONG, CREATOR };

	private readonly HttpClient _Client;
	public string Name { get; } = "AniDB";

	public AniDBGatherer(HttpClient? client = null)
	{
		_Client = client ?? GathererUtils.DefaultGathererClient;
	}

	public async Task<AnimeBase> GetAsync(int id, GatherOptions? options = null)
	{
		var response = await _Client.GetAsync(URL + id).ConfigureAwait(false);
		response.ThrowIfInvalid();

		using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		var doc = new HtmlDocument();
		doc.Load(stream);
		return Parse(doc.DocumentNode, id, options);
	}

	public override string ToString()
		=> Name;

	async Task<IAnimeBase> IAnimeGatherer.GetAsync(int id, GatherOptions? options)
		=> await GetAsync(id, options).ConfigureAwait(false);

	internal AnimeBase Parse(HtmlNode node, int id, GatherOptions? options)
	{
		if (node.Descendants("div").Any(x => x.HasClass("error")))
		{
			throw this.UnableToFind(id);
		}

		return new()
		{
			Id = Get(GetANNId, node, id, "ANN ID"),
			Name = Get(GetTitle, node, id, "title"),
			Songs = new(Get(GetSongs, node, id, "songs").Where(x =>
			{
				return options?.CanBeGathered(x.Type.Type) == true;
			})),
			Year = Get(GetYear, node, id, "year"),
		};
	}

	private static int GetANNId(HtmlNode node)
	{
		var ann = node.Descendants("a")
			.Single(x => x.HasClass("i_resource_ann"));
		var id = ann.Attributes["href"].Value;
		return int.Parse(id.Split("id=")[1]);
	}

	private static IEnumerable<Song> GetSongs(HtmlNode node)
	{
		var dict = new Dictionary<string, string>(2);
		var songType = default(SongType?);
		var songCount = 0;
		foreach (var tr in node.Descendants("tr"))
		{
			foreach (var td in tr.Descendants("td"))
			{
				foreach (var @class in td.GetClasses())
				{
					var text = td.InnerText;
					if (SongProperties.Contains(@class))
					{
						dict.Add(@class, HttpUtility.HtmlDecode(text.Trim()));
					}
					else if (@class == RELTYPE
						&& Enum.TryParse<SongType>(text.Split()[0], true, out var temp))
					{
						songType = temp;
						songCount = 0;
					}
				}
			}

			if (songType.HasValue && dict.Count == 2)
			{
				yield return new Song
				{
					Type = new(songType.Value, ++songCount),
					Name = dict[SONG]!,
					Artist = dict[CREATOR]!,
				};
			}
			dict.Clear();
		}
	}

	private static string GetTitle(HtmlNode node)
	{
		var tab1 = node.Descendants("div")
			.Single(x => x.Id == "tab_1_pane");
		var name = tab1.Descendants("span")
			.Single(x => x.GetAttributeValue("itemprop", null) is "name");
		return name.InnerText.Trim();
	}

	private static int GetYear(HtmlNode node)
	{
		var date = node.Descendants("span").Single(x =>
		{
			var itemProp = x.GetAttributeValue("itemprop", null);
			return itemProp is "datePublished" or "startDate";
		});
		var year = date.Attributes["content"].Value;
		return DateTime.Parse(year).Year;
	}

	private T Get<T>(Func<HtmlNode, T> func, HtmlNode node, int id, string property)
	{
		try
		{
			return func(node);
		}
		catch (Exception e)
		{
			throw this.InvalidPropertyProvided(id, property, e);
		}
	}
}