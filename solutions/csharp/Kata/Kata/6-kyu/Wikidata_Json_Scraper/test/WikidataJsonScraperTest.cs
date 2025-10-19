using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Kata._6_kyu.Wikidata_Json_Scraper.src;

namespace Kata._6_kyu.Wikidata_Json_Scraper.test;

public class WikidataJsonScraperTest
{
    private static async Task TestPage(string id, Dictionary<string, string> expected)
    {
        Assert.That(await WikidataJsonScraper.WikidataScraper($"https://www.wikidata.org/wiki/Special:EntityData/{id}.json"),
            Is.EquivalentTo(expected));
    }

    [TestFixture(Description = "Fixed Tests")]
    public class FixedTests
    {
        [Test(Description = "Example Page (Q42)"), Order(1)]
        public async Task FirstPage()
        {
            var expected = new Dictionary<string, string>
            {
                { "ID", "Q42" },
                { "LABEL", "Douglas Adams" },
                { "DESCRIPTION", "English science fiction writer and humorist (1952–2001)" }
            };
            await TestPage("Q42", expected);
        }

        // [Test(Description = "Cool pages containing all needed information"), Order(2)]
        // public async Task CoolPages()
        // {
        //     var expected1 = new Dictionary<string, string>
        //     {
        //         { "ID", "Q2" },
        //         { "LABEL", "Earth" },
        //         { "DESCRIPTION", "third planet from the Sun in the Solar System" }
        //     };
        //     await TestPage("Q2", expected1);
        //     var expected2 = new Dictionary<string, string>
        //     {
        //         { "ID", "Q513" },
        //         { "LABEL", "Mount Everest" },
        //         {
        //             "DESCRIPTION",
        //             "Earth's highest mountain above sea level, located in the Mahalangur Himal sub-range of the Himalayas"
        //         }
        //     };
        //     await TestPage("Q513", expected2);
        // }
        //
        // [Test(Description = "Edge-Case: No 'en' label"), Order(3)]
        // public async Task MissingLabel()
        // {
        //     var expected = new Dictionary<string, string>
        //     {
        //         { "ID", "Q26013179" },
        //         { "LABEL", "No Label" },
        //         { "DESCRIPTION", "Wikimedia template" }
        //     };
        //     await TestPage("Q26013179", expected);
        // }
        //
        // [Test(Description = "Edge-Case: No 'en' description"), Order(4)]
        // public async Task MissingDesc()
        // {
        //     var expected = new Dictionary<string, string>
        //     {
        //         { "ID", "Q62849465" },
        //         { "LABEL", "BC1G_15416" },
        //         { "DESCRIPTION", "No Description" }
        //     };
        //     await TestPage("Q62849465", expected);
        // }
        //
        // [Test(Description = "Edge-Case: No 'en' description or label"), Order(5)]
        // public async Task MissingBoth()
        // {
        //     var expected = new Dictionary<string, string>
        //     {
        //         { "ID", "Q88627685" },
        //         { "LABEL", "No Label" },
        //         { "DESCRIPTION", "No Description" }
        //     };
        //     await TestPage("Q88627685", expected);
        // }
    }
}