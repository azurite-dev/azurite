using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azurite.Diagnostics;
using Azurite.Wiki.Diagnostics;
using Microsoft.Extensions.Caching.InMemory;
using Polly;
using static Azurite.Helpers;

namespace Azurite.Wiki
{
    public class WikiSearcher : IShipDataProvider
    {
        private readonly string _overrideUrl;
        private HttpClient httpClient {get;} = BuildHttpClient();

        public bool IsLocal => false;

        private static HttpClient BuildHttpClient() {
            return new CachedHttpClient();
        }

        public WikiSearcher()
        {
        }

        public WikiSearcher(HttpClient client) : this()
        {
            httpClient = client;
        }

        public WikiSearcher(string baseUrl)
        {
            _overrideUrl = baseUrl;
        }

        private async Task<string> GetHtml(Uri url)
        {
            try {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0 Azurite/{typeof(WikiSearcher).Assembly.GetName().Version.ToString()}");
                var html = await httpClient.GetStringAsync(url.AbsoluteUri);
                return html;
            }
            catch (Exception e) {
                throw new FetchPageException($"Failed to fetch HTML from request to {url}", e);
            }
        }

        private async Task<string> GetPageHtml(string path) {
            return await GetHtml(new Uri($"{_overrideUrl ?? ConfigConstants.baseUrl}{path.TrimStart('/')}"));
        }

        /// <remarks>
        /// Calling this method invokes ONE page load and ONE parse operations
        /// </remarks>
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

        /// <remarks>
        /// Calling this method invokes ONE page load and TWO parse operations
        /// </remarks>
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
            var stats = html.ExtractFromHtml("shipStats");
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
                        Type = ParseEquipType(e["equip"]?.ToString())
                    };
                }),
                Statistics = ParseStatistics(stats, IsRetrofit(id)),
                Rarity = forceRarity ?? ParseRarity(result["rarity"].ToString()),
                Url = $"{_overrideUrl ?? ConfigConstants.baseUrl}{GetPageNameForShipName(shipName)}",
            };
        }

        private ShipStatistics ParseStatistics(Newtonsoft.Json.Linq.JContainer stats, bool isRetrofit) {
            var ship = new ShipStatistics();
            var applicableStats = isRetrofit
                ? stats.Where(s => s["title"].ToString().Contains("Retrofit"))
                : stats.Where(s => !s["title"].ToString().Contains("Retrofit"));
            ship.Level100 = applicableStats.FirstOrDefault(s => s["title"].ToString().StartsWith("Level 100"))?.ToStatistics();
            ship.Level120 = applicableStats.FirstOrDefault(s => s["title"].ToString().StartsWith("Level 120"))?.ToStatistics();
            ship.Base = isRetrofit ? null : stats.FirstOrDefault(s => s["title"].ToString().Contains("Base")).ToStatistics();
            return ship;            
        }

        private string ParseEquipType(string typeText, bool isRetrofit = false) {
            if (string.IsNullOrWhiteSpace(typeText)) return "";
            var split = typeText.Split('/').Select(s => s.Trim());
            return isRetrofit
                ? split.Last()
                : split.FirstOrDefault() ?? "";
        }

        /// <summary>
        /// Gets the details of a given ship.
        /// </summary>
        /// <remarks>
        /// This will *not* validate that a ship name is correct first! 
        /// Always use a result from <see cref="WikiSearcher.GetShipList"/> to get details. 
        /// Calling this method invokes up to FOUR page loads and FIVE parse operations.
        /// </remarks>
        /// <param name="shipName">A valid ship name.</param>
        /// <returns>A Task for the details of the requested ship.</returns>
        public async Task<IEnumerable<Ship>> GetShipDetails(string shipName) {
            var ships = (await GetShipList()).Where(s => s.Name.Equals(shipName, StringComparison.InvariantCultureIgnoreCase));
            return ships.Select(s => GetShipDetails(s).Result);
            // return new[] {await GetShipDetails(shipName, null, null)};
        }

        /// <summary>
        /// Gets the details of a given ship.
        /// </summary>
        /// <remarks>
        /// This will *not* validate that a ship name is correct first! 
        /// Always use a result from <see cref="WikiSearcher.GetShipList"/> to get details.
        /// Calling this method invokes ONE page load and TWO parse operations.
        /// </remarks>
        /// <param name="summary">A ship <see cref="Azurite.ShipSummary"/> to get details for.</param>
        /// <returns>A Task for the details of the requested ship.</returns>
        public async Task<Ship> GetShipDetails(ShipSummary summary) {
            return await GetShipDetails(summary.Name, summary.Id, ParseRarity(summary.Rarity));
        }

        internal static string GetPageNameForShipName(string shipName) {
            //TODO: this needs to cover more edge cases
            return shipName.Replace(" ", "_").TrimStart('/');
        }
    }
}
