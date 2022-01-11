using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Gatherers;
using SongProcessor.Models;

using System.Xml.Linq;

namespace SongProcessor.Tests.Gatherers;

[TestClass]
public sealed class ANNGatherer_Tests : Gatherer_TestsBase<ANNGatherer>
{
	private const int ID = 13888;
	private const string XML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ann>
   <anime id=""13888"" gid=""2335879489"" type=""TV"" name=""Jormungand"" precision=""TV"" generated-on=""2022-01-10T03:39:38Z"">
      <info gid=""1700307189"" type=""Main title"" lang=""DE"">Jormungand</info>
      <info gid=""97063932"" type=""Vintage"">2012-04-10 to 2012-06-26</info>
      <info gid=""2253194512"" type=""Opening Theme"">""Borderland"" by Mami Kawada</info>
      <info gid=""3026477928"" type=""Ending Theme"">#1: ""Ambivalentidea"" by Nagi Yanagi</info>
      <info gid=""3453345228"" type=""Ending Theme"">#2: ""Shiroku Yawaraka na Hana"" by Nagi Yanagi (ep 4)</info>
   </anime>
</ann>
";
	private const string XML_FAILURE = @"<ann>
	<warning>no result for anime=abcd</warning>
</ann>
";
	protected override IAnimeBase ExpectedAnimeBase { get; } = new AnimeBase
	{
		Id = ID,
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
	protected override ANNGatherer Gatherer { get; } = new();

	[TestMethod]
	public void DefaultParsing_Test()
	{
		var actual = Gatherer.Parse(ID, GatherOptions, XElement.Parse(XML));
		actual.Should().BeEquivalentTo(ExpectedAnimeBase);
	}

	[TestMethod]
	[TestCategory(WEB_CALL_CATEGORY)]
	public async Task Gather_Test()
		=> await AssertRetrievedMatchesAsync(ID).ConfigureAwait(false);

	[TestMethod]
	public void NotFoundParsing_Test()
	{
		Action parse = () => Gatherer.Parse(ID, GatherOptions, XElement.Parse(XML_FAILURE));
		parse.Should().Throw<KeyNotFoundException>();
	}
}