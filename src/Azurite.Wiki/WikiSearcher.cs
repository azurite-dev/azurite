using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azurite.Wiki.Diagnostics;
using Polly;
using static Azurite.Helpers;

namespace Azurite.Wiki
{
    public class WikiSearcher : WikiClientBase, IShipDataProvider
    {
        public WikiSearcher()
        {
        }

        public WikiSearcher(HttpClient client) : base(client)
        {
        }

        public WikiSearcher(string baseUrl) : base(baseUrl)
        {
        }

        private async Task<string> GetPageHtml(string path) {
            return await GetHtml(new Uri($"{_overrideUrl ?? ConfigConstants.baseUrl}{path.TrimStart('/')}"));
        }

        public bool UseMobileView {get;set;} = true;

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
            var result = html.ExtractFromHtml("shipDetails.mobile");
            var stats = html.ExtractFromHtml("shipStats.mobile");
            var name = result["name"].ToString().Before(" - ");
            if (!string.IsNullOrWhiteSpace(result["hidden_header"]?.ToString())
                || string.IsNullOrWhiteSpace(result["name"]?.ToString()) 
                || string.IsNullOrWhiteSpace(result["header"]?.ToString())
            ) {
                throw new Diagnostics.ShipNotFoundException(name ?? shipName); 
            }
            // var t = result["build_time"].ToString();
            var t = result["build_time"]?.ToString() ?? result["build"]?.ToString() ?? "";
            var id = forceId ?? result["description"].ToString().Split('(',')')[1];
            // var type = result["description"].ToString().Before("(").Split(result["faction_name"]).Last();
            var typeFull = result["type_main"].ToString();
            // ↓↓ this is terrible: it's pretty specific to the mobile layout and I need to look at improving the way multi-classification ships are handled
            (string type, string subType) = typeFull.Contains("→")
                ? (result["type_main"].ToString().Split('→').First(), result["type_main"].ToString().Split('→').Last())
                : (typeFull, null);
            var rarity = forceRarity ?? ParseRarity(result["rarity_category"].ToString().Split(':', ' ').Skip(1).First()); // this is also bad and will fail on non-mobile HTML
            int slot = 0;
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
                Type = (IsRetrofit(id) ? subType ?? type : type).Trim().ToTitleCase(),
                //Subtype = result["type_sub"]?.ToString(),
                Class = result["class"].ToString().Replace("-class", ""),
                Stars = rarity.ToStarRating(),
                Faction = new Faction {
                    Name = result["faction_name"].ToString(),  
                    Prefix = result["name_full"].ToString().Before(name)
                },
                Equipment = result["equipment"].Select(e => {
                    slot++;
                    return new EquipmentSlot {
                        Slot = slot,
                        Efficiency = e["efficiency"]?.ToString() ?? "",
                        Type = ParseEquipType(e["equip"], IsRetrofit(id))
                    };
                }),
                Statistics = ParseStatistics(stats, IsRetrofit(id)),
                Rarity = rarity,
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

        private string ParseEquipType(Newtonsoft.Json.Linq.JToken typeText, bool isRetrofit = false) {
            if (string.IsNullOrWhiteSpace(typeText?.ToString())) return "";
            if (typeText is Newtonsoft.Json.Linq.JArray) {
                var equips = typeText.Select(t => t?.ToString() ?? "").Where(e => !string.IsNullOrWhiteSpace(e));
                return (isRetrofit
                    // ? string.Join(" / ", equips.Select(e => e.Replace("Retrofit", "", true, System.Globalization.CultureInfo.InvariantCulture).Trim('(', ')').Trim()))
                    ? string.Join(" / ", equips)
                    : string.Join(" / ", equips.Where(e => !e.ToLower().Contains("retrofit"))))
                    ?? "";
            } else {
                var split = typeText?.ToString().Split('/').Select(s => s.Trim());
                return isRetrofit
                    ? split.Last()
                    : split.FirstOrDefault() ?? "";
            }
            
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
            if (string.IsNullOrWhiteSpace(summary.Name)) {
                var ship = (await GetShipList()).FirstOrDefault(s => s.Id.Equals(summary.Id, StringComparison.InvariantCultureIgnoreCase));
                return await GetShipDetails(ship);
            }
            return await GetShipDetails(summary.Name, summary.Id, ParseRarity(summary.Rarity));
        }

        internal static string GetPageNameForShipName(string shipName) {
            //TODO: this needs to cover more edge cases
            return shipName.Replace(" ", "_").TrimStart('/');
        }
    }
}
