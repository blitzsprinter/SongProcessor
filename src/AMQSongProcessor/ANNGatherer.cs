using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using AdvorangesUtils;

using AMQSongProcessor.Models;

namespace AMQSongProcessor
{
	public static class ANNGatherer
	{
		private const string ARTIST = "arist";
		private const string NAME = "name";
		private const string POSITION = "position";
		private const string URL = "https://cdn.animenewsnetwork.com/encyclopedia/api.xml?anime=";

		private static readonly HttpClient _Client = new HttpClient();

		private static readonly string SongPattern =
			$@"(?<{POSITION}>\d*?)" + //Some openings/endings will have a position
			  "(: )?" + //The position will be followed up with a colon and space
			$@"""(?<{NAME}>.+?)""" + //The name will be in quotes
			  ".+?by " + //The name may have a translation in parans, and will be followed with by
			 $"(?<{ARTIST}>.+?)" + //The artist is just a simple match of any characters
			 @"( \(eps|$)"; //The artist ends at (eps ###-###) or the end of the line

		private static readonly Regex SongRegex =
			new Regex(SongPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		private static readonly string[] VintageFormats = new[]
		{
			"yyyy"
		};

		public static async Task<Anime> GetAsync(int id, ANNGathererOptions? options = null)
		{
			var url = URL + id;
			var result = await _Client.GetAsync(url).CAF();
			if (!result.IsSuccessStatusCode)
			{
				throw new HttpRequestException($"{url} threw {result.StatusCode}.");
			}

			var stream = await result.Content.ReadAsStreamAsync().CAF();
			var doc = XElement.Load(stream);
			if (doc.Descendants("warning").Any(x => x.Value.Contains("no result for anime")))
			{
				throw new HttpRequestException($"{id} does not exist on ANN.");
			}

			var anime = new Anime
			{
				Id = id,
				Year = int.MaxValue,
			};

			foreach (var element in doc.Descendants("info"))
			{
				var type = element.Attribute("type").Value.ToLower();
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

		private static void ProcessSong(Anime anime, ANNGathererOptions? options, XElement e, string t)
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
				Type = new SongTypeAndPosition(type, position),
				Name = match.Groups[NAME].Value,
				Artist = match.Groups[ARTIST].Value,
			});
		}

		private static void ProcessTitle(Anime anime, XElement e)
			=> anime.Name = e.Value;

		private static void ProcessVintage(Anime anime, XElement e)
		{
			static bool TryParseExact(string s, out DateTime dt)
				=> DateTime.TryParseExact(s, VintageFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);

			var s = e.Value.Split(' ')[0];
			if (DateTime.TryParse(s, out var dt) || TryParseExact(s, out dt))
			{
				anime.Year = Math.Min(anime.Year, dt.Year);
			}
		}
	}

	public sealed class ANNGathererOptions
	{
		public bool AddEndings { get; set; }
		public bool AddInserts { get; set; }
		public bool AddOpenings { get; set; }
		public bool AddSongs { get; set; }

		public bool CanBeGathered(SongType type)
		{
			return AddSongs && type switch
			{
				SongType.Ed => AddEndings,
				SongType.In => AddInserts,
				SongType.Op => AddOpenings,
				_ => throw new ArgumentOutOfRangeException(nameof(type)),
			};
		}
	}
}