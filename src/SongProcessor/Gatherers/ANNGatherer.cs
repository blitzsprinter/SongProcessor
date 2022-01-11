using SongProcessor.Models;

using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SongProcessor.Gatherers;

public sealed class ANNGatherer : IAnimeGatherer
{
	private const string ARTIST = "artist";
	private const string NAME = "name";
	private const string POSITION = "position";
	private const string SongPattern =
		$@"(?<{POSITION}>\d*?)" + //Some openings/endings will have a position
		"(: )?" + //The position will be followed up with a colon and space
		$@"""(?<{NAME}>.+?)""" + //The name will be in quotes
		".+?by " + //The name may have a translation in parans, and will be followed with by
		$"(?<{ARTIST}>.+?)" + //The artist is just a simple match of any characters
		@"( \(eps?|$)"; //The artist ends at (eps/ep ###-###) or the end of the line
	private const string URL = "https://cdn.animenewsnetwork.com/encyclopedia/api.xml?anime=";

	private static readonly Regex SongRegex
		= new(SongPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
	private static readonly string[] VintageFormats = new[]
	{
		"yyyy"
	};

	private readonly HttpClient _Client;
	public string Name { get; } = "ANN";

	public ANNGatherer(HttpClient? client = null)
	{
		_Client = client ?? new HttpClient();
	}

	public async Task<AnimeBase> GetAsync(int id, GatherOptions? options = null)
	{
		var url = URL + id;
		var result = await _Client.GetAsync(url).ConfigureAwait(false);
		if (!result.IsSuccessStatusCode)
		{
			throw new HttpRequestException($"{url} threw {result.StatusCode}.");
		}

		var stream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
		var doc = XElement.Load(stream);
		return Parse(id, options, doc);
	}

	public override string ToString()
		=> Name;

	async Task<IAnimeBase> IAnimeGatherer.GetAsync(int id, GatherOptions? options)
		=> await GetAsync(id, options).ConfigureAwait(false);

	internal AnimeBase Parse(int id, GatherOptions? options, XElement doc)
	{
		if (doc.Descendants("warning").Any(x => x.Value.Contains("no result", StringComparison.OrdinalIgnoreCase)))
		{
			throw new KeyNotFoundException($"{id} cannot be found in {Name}.");
		}

		var anime = new AnimeBase
		{
			Id = id,
			Year = int.MaxValue,
		};

		foreach (var element in doc.Descendants("info"))
		{
			var attr = element.Attribute("type");
			if (attr is null)
			{
				continue;
			}

			var type = attr.Value.ToLower();
			try
			{
				switch (type)
				{
					case "main title":
						ProcessTitle(anime, element);
						break;

					case "vintage":
						ProcessVintage(anime, element);
						break;

					case "opening theme":
					case "ending theme":
					case "insert song":
						ProcessSong(anime, options, element, type);
						break;
				}
			}
			catch (Exception e)
			{
				throw new FormatException($"Invalid {type} provided by ANN for {id}", e);
			}
		}
		return anime;
	}

	private static void ProcessSong(AnimeBase anime, GatherOptions? options, XElement e, string t)
	{
		var type = Enum.Parse<SongType>(t.Split(' ')[0], true);
		if (options?.CanBeGathered(type) == false)
		{
			return;
		}

		var match = SongRegex.Match(e.Value);
		var position = match.Groups.TryGetValue(POSITION, out var a)
			&& int.TryParse(a.Value, out var temp) ? temp : default(int?);
		anime.Songs.Add(new Song
		{
			Type = new(type, position),
			Name = match.Groups[NAME].Value,
			Artist = match.Groups[ARTIST].Value,
		});
	}

	private static void ProcessTitle(AnimeBase anime, XElement e)
		=> anime.Name = e.Value;

	private static void ProcessVintage(AnimeBase anime, XElement e)
	{
		var s = e.Value.Split(' ')[0];
		if (DateTime.TryParse(s, out var dt)
			|| DateTime.TryParseExact(s, VintageFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
		{
			anime.Year = Math.Min(anime.Year, dt.Year);
		}
	}
}