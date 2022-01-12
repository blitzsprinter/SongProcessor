using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Gatherers;
using SongProcessor.Models;

using System.Net;
using System.Xml.Linq;

namespace SongProcessor.Tests.Gatherers;

[TestClass]
public sealed class ANNGatherer_Tests : Gatherer_TestsBase<ANNGatherer>
{
	private const int ANN_ID = 13888;
	private const string XML_NOT_FOUND = @"<ann>
	<warning>no result for anime=abcd</warning>
</ann>
";
	private const string XML_SUCCESS = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ann>
   <anime id=""13888"" gid=""2335879489"" type=""TV"" name=""Jormungand"" precision=""TV"" generated-on=""2022-01-10T03:39:38Z"">
      <info gid=""1700307189"" type=""Main title"" lang=""DE"">Jormungand</info>
      <info gid=""97063932"" type=""Vintage"">2012-04-10 to 2012-06-26</info>
      <info gid=""2253194512"" type=""Opening Theme"">""Borderland"" by Mami Kawada</info>
      <info gid=""3026477928"" type=""Ending Theme"">#1: ""Ambivalentidea"" by Nagi Yanagi</info>
      <info gid=""3453345228"" type=""Ending Theme"">#2: ""Shiroku Yawaraka na Hana"" by Nagi Yanagi (ep 4)</info>
      <info gid=""3453345229"">empty</info>
   </anime>
</ann>
";

	protected override IAnimeBase ExpectedAnimeBase { get; } = new AnimeBase
	{
		Id = ANN_ID,
		Name = "Jormungand",
		Songs = new()
		{
			new()
			{
				Artist = "Mami Kawada",
				Name = "Borderland",
				Type = SongType.Op.Create(null),
			},
			new()
			{
				Artist = "Nagi Yanagi",
				Name = "Ambivalentidea",
				Type = SongType.Ed.Create(1),
			},
			new()
			{
				Artist = "Nagi Yanagi",
				Name = "Shiroku Yawaraka na Hana",
				Type = SongType.Ed.Create(2),
			},
			// ANN doesn't have the inserts documented for this
		},
		Source = null,
		Year = 2012
	};
	protected override ANNGatherer Gatherer { get; set; } = new();

	[TestMethod]
	public void DefaultParsing_Test()
	{
		var actual = Gatherer.Parse(XElement.Parse(XML_SUCCESS), ANN_ID, GatherOptions);
		actual.Should().BeEquivalentTo(ExpectedAnimeBase);
	}

	[TestMethod]
	[TestCategory(WEB_REQUEST_CATEGORY)]
	public async Task Gather_Test()
		=> await AssertRetrievedMatchesAsync(ANN_ID).ConfigureAwait(false);

	[TestMethod]
	public async Task InvalidStatusCode_Test()
	{
		Gatherer = new ANNGatherer(new HttpClient(new HttpTestHandler
		{
			StatusCode = HttpStatusCode.Forbidden,
		}));

		Func<Task> request = () => Gatherer.GetAsync(73, GatherOptions);
		await request.Should().ThrowAsync<HttpRequestException>().ConfigureAwait(false);
	}

	[TestMethod]
	public void NotFoundParsing_Test()
	{
		Action parse = () => Gatherer.Parse(XElement.Parse(XML_NOT_FOUND), ANN_ID, GatherOptions);
		parse.Should().Throw<KeyNotFoundException>();
	}

	[TestMethod]
	public void ToString_Test()
		=> Gatherer.ToString().Should().Be("ANN");
}