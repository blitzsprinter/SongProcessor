using SongProcessor.Models;

using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SongProcessor.Gatherers;

public sealed class ANNGatherer(HttpClient? client = null) : IAnimeGatherer
{
	private const string ARTIST = "artist";
	private const string NAME = "name";
	private const string POSITION = "position";
	private const string SONG_PATTERN =
		$@"(?<{POSITION}>\d*?)" + // Some openings/endings will have a position
		"(: )?" + // Position will be followed up with a colon and space
		$@"""(?<{NAME}>.+?)""" + // Name will be in quotes
		".+?by " + // Name may have a translation in parans, and will be followed with 'by'
		$"(?<{ARTIST}>.+?)" + // Artist is just a simple match of any characters
		@"( \(eps?|$)"; // Artist ends at (eps/ep ###-###) or the end of the line
	private const string URL =
		"https://cdn.animenewsnetwork.com/encyclopedia/api.xml?anime=";

	private static readonly Regex SongRegex =
		new(SONG_PATTERN, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
	private static readonly string[] VintageFormats =
	[
		"yyyy"
	];

	private readonly HttpClient _Client = client ?? GathererUtils.DefaultGathererClient;
	public string Name { get; } = "ANN";

	public async Task<AnimeBase> GetAsync(int id, GatherOptions options)
	{
		using var response = await _Client.GetAsync(URL + id).ConfigureAwait(false);
		response.EnsureSuccessStatusCode();

		XElement doc;
		await using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
		{
			doc = XElement.Load(stream);
		}
		return Parse(doc, id, options);
	}

	public override string ToString()
		=> Name;

	async Task<IAnimeBase> IAnimeGatherer.GetAsync(int id, GatherOptions options)
		=> await GetAsync(id, options).ConfigureAwait(false);

	internal AnimeBase Parse(XElement element, int id, GatherOptions options)
	{
		if (element.Descendants("warning").Any(x => x.Value.Contains("no result")))
		{
			throw this.UnableToFind(id);
		}

		var anime = new AnimeBase
		{
			Id = id,
			Year = int.MaxValue,
		};

		foreach (var info in element.Descendants("info"))
		{
			var type = info.Attribute("type")?.Value?.ToLower();
			if (type is null)
			{
				continue;
			}

			try
			{
				switch (type)
				{
					case "main title":
						anime.Name = info.Value.Trim();
						break;

					case "vintage":
						anime.Year = Math.Min(GetYear(info), anime.Year);
						break;

					case "opening theme":
					case "ending theme":
					case "insert song":
						var songType = Enum.Parse<SongType>(type.Split(' ')[0], true);
						if (options.CanBeGathered(songType))
						{
							anime.Songs.Add(GetSong(info, songType));
						}
						break;
				}
			}
			catch (Exception e)
			{
				throw this.InvalidPropertyProvided(id, type, e);
			}
		}
		return anime;
	}

	private static Song GetSong(XElement element, SongType type)
	{
		var groups = SongRegex.Match(element.Value).Groups;
		var position = groups.TryGetValue(POSITION, out var pos)
			&& int.TryParse(pos.Value, out var temp) ? temp : default(int?);
		return new Song
		{
			Type = new(type, position),
			Name = groups[NAME].Value,
			Artist = groups[ARTIST].Value,
		};
	}

	private static int GetYear(XElement element)
	{
		var s = element.Value.Split(' ')[0];
		if (DateTime.TryParse(s, out var dt) || DateTime.TryParseExact(s, VintageFormats,
			CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
		{
			return dt.Year;
		}
		return int.MaxValue;
	}
}