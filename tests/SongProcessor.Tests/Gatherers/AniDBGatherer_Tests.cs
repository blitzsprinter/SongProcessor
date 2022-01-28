using FluentAssertions;

using HtmlAgilityPack;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Gatherers;
using SongProcessor.Models;

using System.Net;

namespace SongProcessor.Tests.Gatherers;

[TestClass]
public sealed class AniDBGatherer_Tests : Gatherer_TestsBase<AniDBGatherer>
{
	private const int ANIDB_ID = 8842;
	private const int ANN_ID = 13888;
	private const string HTML_NOT_FOUND = @"<!DOCTYPE html>
<html lang=""en"" prefix=""og: http://ogp.me/ns#"">
   <body id=""anidb"" class=""anime"">
	  <div id=""layout-content"">
		 <div id=""layout-main"">
			<h1 class=""anime"">ERROR</h1>
			<div class=""g_content anime_all sidebar"">
			   <a href=""/perl-bin/animedb.pl?show=vddb"" style=""display:none;"">Download (Do <b>NOT</b> click!)</a>
			   <div class=""g_msg g_bubble error"">
				  <h3>ERROR</h3>
				  <div class=""container"">Unknown anime id.<br/>This entry may have been deleted from the database.
				  </div>
			   </div>
			</div>
			<div class=""g_end""></div>
		 </div>
	  </div>
   </body>
</html>
";
	private const string HTML_SUCCESS = @"<!DOCTYPE html>
<html lang=""en"" prefix=""og: http://ogp.me/ns#"">
   <body id=""anidb"" class=""anime"">
	  <div id=""layout-content"">
		 <div id=""layout-main"">
			<div class=""g_content anime_all sidebar"" itemscope itemtype=""https://schema.org/TVSeries"">
			   <div class=""g_section info"">
				  <div class=""block"">
					 <div class=""data"">
						<div id=""tabbed_pane"" class=""tabbed_pane"">
						   <div class=""g_bubble body"">
							  <div id=""tab_1_pane"" class=""pane info"">
								 <div class=""g_definitionlist"">
									<table>
									   <tbody>
										  <tr class=""g_odd romaji"">
											 <td class=""value"">
												<span itemprop=""name"">Jormungand</span>
											 </td>
										  </tr>
										  <tr class=""g_odd year"">
											 <td class=""value""><span itemprop=""startDate"" content=""2012-04-11"">11.04.2012</span> until <span itemprop=""endDate"" content=""2012-06-27"">27.06.2012</span></td>
										  </tr>
										  <tr class=""g_odd resources"">
											 <td class=""value"">
												<div class=""group thirdparty english"">
												   <div class=""icons""><a class=""i_icon i_resource_ann brand"" href=""https://www.animenewsnetwork.com/encyclopedia/anime.php?id=13888"" data-anidb-rel=""anidb::extern"" itemprop=""sameAs"" title=""ANN""><span class=""text"">ANN</span></a></div>
												</div>
											 </td>
										  </tr>
									   </tbody>
									</table>
								 </div>
							  </div>
						   </div>
						</div>
					 </div>
				  </div>
			   </div>
			   <div id=""tabbed_pane_main_6"" class=""tabbed_pane resized g_section tabbed_pane_main"">
				  <div class=""g_bubble body"">
					 <div id=""tab_main_6_3_pane"" class=""pane hide songs"">
						<div class=""g_section songs"">
						   <div class=""container "">
							  <table id=""songlist"" class=""songlist"">
								 <tbody>
									<tr id=""relationid_20601"" class=""g_odd"">
									   <td rowspan=""4"" class=""reltype"">opening</td>
									   <td rowspan=""4"" class=""name song""><a href=""/song/51351"">Borderland</a></td>
									   <td class=""name creator""><a class=""normal"" href=""/creator/709"">Kawada Mami</a></td>
									</tr>
									<tr id=""relationid_20999"">
									   <td rowspan=""11"" class=""reltype"">ending</td>
									   <td rowspan=""5"" class=""name song""><a href=""/song/51352"">Ambivalentidea</a></td>
									   <td class=""name creator""><a class=""normal"" href=""/creator/29595"">Yanagi Nagi</a></td>
									</tr>
									<tr  class=""rowspan"">
									   <td rowspan=""6"" class=""name song""><a href=""/song/52139"">Shiroku Yawaraka na Hana</a></td>
									   <td class=""name creator""><a class=""normal"" href=""/creator/29595"">Yanagi Nagi</a></td>
									</tr>
									<tr id=""relationid_41949"" class=""g_odd"">
									   <td rowspan=""13"" class=""reltype"">insert song</td>
									   <td rowspan=""4"" class=""name song""><a href=""/song/53878"">Time to Rock and Roll</a></td>
									   <td class=""name creator""><a class=""virtual"" href=""/creator/virtual/196213"">SANTA</a></td>
									</tr>
									<tr  class=""rowspan"">
									   <td rowspan=""1"" class=""name song""><a href=""/song/52140"">Tosca-Vissi D`Arte, Vissi D`Amore</a></td>
									   <td class=""name creator""><a class=""normal"" href=""/creator/32618"">Giacomo Puccini</a></td>
									</tr>
									<tr  class=""g_odd rowspan"">
									   <td rowspan=""4"" class=""name song""><a href=""/song/53900"">Time to Attack</a></td>
									   <td class=""name creator""><a class=""virtual"" href=""/creator/virtual/196215"">SANTA</a></td>
									</tr>
									<tr  class=""rowspan"">
									   <td rowspan=""4"" class=""name song""><a href=""/song/53899"">Meu Mundo Amor</a></td>
									   <td class=""name creator""><a class=""virtual"" href=""/creator/virtual/196217"">Silvio Anastacio</a></td>
									</tr>
									<tr id=""relationid_21628"">
									   <td rowspan=""4"" class=""reltype"">background music</td>
									   <td rowspan=""4"" class=""name song""><a href=""/song/53877"">Jormungand</a></td>
									   <td class=""name creator""><a class=""normal"" href=""/creator/30323"">Fukuoka Yutaka</a></td>
									</tr>
								 </tbody>
							  </table>
						   </div>
						</div>
					 </div>
				  </div>
			   </div>
			</div>
		 </div>
	  </div>
   </body>
</html>
";

	protected override IAnimeBase ExpectedAnimeBase { get; } = new AnimeBase
	{
		Id = ANN_ID,
		Name = "Jormungand",
		Songs = new()
		{
			new()
			{
				Artist = "Kawada Mami",
				Name = "Borderland",
				Type = SongType.Op.Create(1),
			},
			new()
			{
				Artist = "Yanagi Nagi",
				Name = "Ambivalentidea",
				Type = SongType.Ed.Create(1),
			},
			new()
			{
				Artist = "Yanagi Nagi",
				Name = "Shiroku Yawaraka na Hana",
				Type = SongType.Ed.Create(2),
			},
			new()
			{
				Artist = "SANTA",
				Name = "Time to Rock and Roll",
				Type = SongType.In.Create(null),
			},
			new()
			{
				Artist = "Giacomo Puccini",
				Name = "Tosca-Vissi D\u0060Arte, Vissi D\u0060Amore",
				Type = SongType.In.Create(null),
			},
			new()
			{
				Artist = "SANTA",
				Name = "Time to Attack",
				Type = SongType.In.Create(null),
			},
			new()
			{
				Artist = "Silvio Anastacio",
				Name = "Meu Mundo Amor",
				Type = SongType.In.Create(null),
			},
			new()
			{
				Artist = "Fukuoka Yutaka",
				Name = "Jormungand",
				Type = SongType.In.Create(null),
			},
		},
		Source = null,
		Year = 2012
	};
	protected override AniDBGatherer Gatherer { get; set; } = new();

	[TestMethod]
	public void DefaultParsing_Test()
	{
		var doc = new HtmlDocument();
		doc.LoadHtml(HTML_SUCCESS);
		var actual = Gatherer.Parse(doc.DocumentNode, ANIDB_ID, GatherOptions);
		actual.Should().BeEquivalentTo(ExpectedAnimeBase);
	}

	[TestMethod]
	[TestCategory(WEB_REQUEST_CATEGORY)]
	public async Task Gather_Test()
		=> await AssertRetrievedMatchesAsync(ANIDB_ID).ConfigureAwait(false);

	[TestMethod]
	public async Task InvalidStatusCode_Test()
	{
		Gatherer = new AniDBGatherer(new HttpClient(new HttpTestHandler
		{
			StatusCode = HttpStatusCode.Forbidden,
		}));

		Func<Task> request = () => Gatherer.GetAsync(73, GatherOptions);
		await request.Should().ThrowAsync<HttpRequestException>().ConfigureAwait(false);
	}

	[TestMethod]
	public void NotFoundParsing_Test()
	{
		var doc = new HtmlDocument();
		doc.LoadHtml(HTML_NOT_FOUND);
		Action parse = () => Gatherer.Parse(doc.DocumentNode, 8842, GatherOptions);
		parse.Should().Throw<KeyNotFoundException>();
	}

	[TestMethod]
	public void ToString_Test()
		=> Gatherer.ToString().Should().Be("AniDB");
}