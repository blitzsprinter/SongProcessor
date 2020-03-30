using System;
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

		public static async Task<Anime> GetAsync(int id)
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

			void ProcessTitle(XElement e, string _)
				=> anime.Name = e.Value;

			void ProcessVintage(XElement e, string _)
			{
				var dt = DateTime.Parse(e.Value.Split(' ')[0]);
				anime.Year = Math.Min(anime.Year, dt.Year);
			}

			void ProcessSong(XElement e, string t)
			{
				var match = SongRegex.Match(e.Value);
				var type = Enum.Parse<SongType>(t.Split(' ')[0], true);
				var position = match.Groups.TryGetValue(POSITION, out var a)
					&& int.TryParse(a.Value, out var temp) ? temp : default(int?);
				anime.Songs.Add(new Song
				{
					Type = new SongTypeAndPosition(type, position),
					Name = match.Groups[NAME].Value,
					Artist = match.Groups[ARTIST].Value,
				});
			}

			foreach (var info in doc.Descendants("info"))
			{
				var type = info.Attribute("type").Value.ToLower();
				try
				{
					var f = type switch
					{
						"main title" => ProcessTitle,
						"vintage" => ProcessVintage,
						"opening theme" => ProcessSong,
						"ending theme" => ProcessSong,
						"insert song" => ProcessSong,
						_ => default(Action<XElement, string>)
					};
					f?.Invoke(info, type);
				}
				catch (Exception e)
				{
					throw new FormatException($"Invalid {type} provided by ANN for {id}", e);
				}
			}

			return anime;
		}
	}
}