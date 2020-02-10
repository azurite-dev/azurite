using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azurite.Diagnostics;
using Azurite.Wiki.Diagnostics;
using OpenScraping;
using OpenScraping.Config;
using Polly;
using static Azurite.Helpers;

namespace Azurite.Wiki
{
    public class WikiSearcher : IShipDataProvider
    {
        private readonly string _overrideUrl;

        public bool IsLocal => false;

        public WikiSearcher()
        {
            
        }

        public WikiSearcher(string baseUrl)
        {
            _overrideUrl = baseUrl;
        }

        private static async Task<string> GetHtml(Uri url)
        {
            try {
                using (var client = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = true
                }))
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0 Azurite/0.0.1");
                    var html = await client.GetStringAsync(url.AbsoluteUri);
                    return html;
                }
            }
            catch (Exception e) {
                throw new FetchPageException($"Failed to fetch HTML from request to {url}", e);
            }
        }

        private async Task<string> GetPageHtml(string path) {
            return await GetHtml(new Uri($"{_overrideUrl ?? ConfigConstants.baseUrl}{path.TrimStart('/')}"));
        }

        public async Task<List<ShipSummary>> GetShipList() {
            var html = await GetPageHtml("List_of_Ships");
            var result = html.ExtractFromHtml("shipList");
            if (result?["ships"] == null)
            {
                throw new HtmlParseException("Errors encountered while parsing initial search results!");
            }
            var ships = new List<ShipSummary>();
            foreach (var ship in result["ships"].Where(s => s.Count() != 0))
            {
                var id = ship["id"].ToString();
                var summary = new ShipSummary {
                    Id = id,
                    Name = ship["name"].ToString(),
                    Rarity = ParseRarity(ship["rarity"].ToString()).ToString(),
                    Type = IsRetrofit(id) ? (ship["subtype"]?.ToString() ?? ship["type"].ToString()) : ship["type"].ToString(),
                    // Subtype = ship["subtype"].ToString(),
                    FactionName = ship["faction"].ToString()
                };
                ships.Add(summary);
            }
            return ships;
        }

        /// <param name="forceId">Forces using the specified ID rather than the scraped one. Used to differentiate retrofits.</param>
        /// <param name="shipName">Name of the ship. Not validated.</param>
        /// <param name="forceRarity">Override detected rarity. Used to differentiate retrofits.</param>
        private async Task<Ship> GetShipDetails(string shipName, string forceId, Rarity? forceRarity) {
            var fetch = Policy
                // .HandleResult<string>(s => string.IsNullOrWhiteSpace(s) || s.Length < 100)
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    2, 
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, delay, count, _) => {
                    System.Console.WriteLine($"Found {ex} on attempt {count}. Retrying after {delay.Seconds}s...");
                });
            var op = await fetch.ExecuteAndCaptureAsync(() => GetPageHtml(GetPageNameForShipName(shipName)));
            if (op.Outcome == OutcomeType.Failure) throw new FetchPageException("Failed to fetch ship details", op.FinalException);
            var html = op.Result;
            var result = html.ExtractFromHtml("shipDetails");
            var name = result["name"].ToString().Before(" - ");
            if (string.IsNullOrWhiteSpace(result["name"]?.ToString()) || string.IsNullOrWhiteSpace(result["header"]?.ToString())) {
                throw new Diagnostics.ShipNotFoundException(name ?? shipName);
            }
            var t = result["build_time"].ToString();
            var id = forceId ?? result["id"].ToString();
            var type = result["type_main"].ToString();
            return new Ship {
                ShipId = id,
                ShipName = new ShipName(
                    name, 
                    result["name_jp"]?.ToString() ?? string.Empty, 
                    result["name_cn"]?.ToString() ?? string.Empty, 
                    result["name_kr"]?.ToString() ?? string.Empty
                ),
                BuildTime = t.ParseTimeSpan(),
                BuildType = t.All(c => char.IsDigit(c) || c == ':') ? "" : t,
                Type = IsRetrofit(id) ? (result["type_sub"]?.ToString() ?? type) : type,
                //Subtype = result["type_sub"]?.ToString(),
                Class = result["class"].ToString(),
                Stars = new Stars { 
                    Default = result["stars"].ToString().Where(c => c == '★').Count(),
                    Max = result["stars"].ToString().Count()
                },
                Faction = new Faction {
                    Name = result["faction_name"].ToString(), 
                    Prefix = result["title_full"].ToString().Before(name)
                },
                Equipment = result["equipment"].Select(e => {
                    return new EquipmentSlot {
                        Slot = int.TryParse(e["slot"].ToString(), out var slot) ? slot : 0,
                        Efficiency = e["efficiency"]?.ToString() ?? "",
                        Type = e["equip"]?.ToString() ?? ""
                    };
                }),
                Rarity = forceRarity ?? ParseRarity(result["rarity"].ToString()),
                Url = $"{_overrideUrl ?? ConfigConstants.baseUrl}{GetPageNameForShipName(shipName)}",
            };
        }

        /// <summary>
        /// Gets the details of a given ship.
        /// </summary>
        /// <remarks>
        /// This will *not* validate that a ship name is correct first! 
        /// Always use a result from <see cref="WikiSearcher.GetShipList"/> to get details.
        /// </remarks>
        /// <param name="shipName">A valid ship name.</param>
        /// <returns>A Task for the details of the requested ship.</returns>
        public async Task<IEnumerable<Ship>> GetShipDetails(string shipName) {
            return new[] {await GetShipDetails(shipName, null, null)};
        }

        public async Task<Ship> GetShipDetails(ShipSummary summary) {
            return await GetShipDetails(summary.Name, summary.Id, ParseRarity(summary.Rarity));
        }

        public static string GetPageNameForShipName(string shipName) {
            //TODO: this needs to cover more edge cases
            return shipName.Replace(" ", "_").TrimStart('/');
        }
    }

    internal static class ScrapingExtensions {
        internal static Newtonsoft.Json.Linq.JContainer ExtractFromHtml(this string html, string resourceName) {
            var config = ScrapingExtensions.CreateConfig(resourceName);
            var scraper = new StructuredDataExtractor(config);
            var result = scraper.Extract(html);
            return result;
        }

        internal static TimeSpan? ParseTimeSpan(this string s) {
            return s.All(c => char.IsDigit(c) || c == ':')
                ? (TimeSpan?)TimeSpan.Parse(s)
                : null;
        }

        internal static string Before(this string s, string separator) {
            return s.Split(new[] { separator }, StringSplitOptions.None).First().Trim();
        }

        private static ConfigSection CreateConfig(string resourceName) {
            var assembly = typeof(WikiSearcher).Assembly;
            string[] names = assembly.GetManifestResourceNames();
            if (!names.Any(n => n.Equals(resourceName))) {
                resourceName = names.FirstOrDefault(n => n.Contains(resourceName));
            }
            var stream = assembly.GetManifestResourceStream(resourceName);
            var reader = new System.IO.StreamReader(stream);
            return StructuredDataConfig.ParseJsonString(reader.ReadToEnd());
        }
    }
}
