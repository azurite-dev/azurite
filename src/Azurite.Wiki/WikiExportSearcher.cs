using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Azurite.Helpers;

namespace Azurite.Wiki
{
    public class WikiExportSearcher : WikiClientBase
    {
        private async Task<string> GetExportPage(string shipName) {
            var url = $"{_overrideUrl ?? ConfigConstants.baseUrl}Special:Export/{shipName.TrimStart('/')}";
            var xml = await httpClient.GetStringAsync(url);
            return xml;
        }

        private async Task<XDocument> GetXmlAsync(string pageName) {
            return XDocument.Parse(await GetExportPage(pageName));
        }

        private string GetPageText(XElement xShip) {
            return xShip.Descendants("text").First().Value.Trim('{', '}').Trim().Replace("Ship ", "");
        }

        public async Task<IEnumerable<Ship>> GetShipDetails(string shipName)
        {
            var xDoc = await GetXmlAsync("shipName");
            var xShip = xDoc.Descendants("page").Where(p => p.Descendants("title").First().Value == shipName).First();
            var content = GetPageText(xShip);
            var props = content
                .SplitOn("<!-------------")
                .ToDictionary(
                    k => k.SplitOn("------------->").First().Trim(), 
                    v => v.SplitOn("------------->").Last().Trim('|').SplitOn(" | ").ToDictionary(sk => sk.SplitOn(" = ").First(), sv => sv.SplitOn(" = ").Last())
                );
            var general = props["S1: General"];
            var t = general.FirstOrDefault(s => s.Key == "ConstructTime");
            var rarity = ParseRarity(general.GetValue("Rarity"));
            var ship = new Ship {
                ShipId = general.FirstOrDefault(s => s.Key == "ID").Value,
                ShipName = new ShipName {
                    EN = shipName,
                    CN = general.FirstOrDefault(s => s.Key == "CNName").Value,
                    JP = general.FirstOrDefault(s => s.Key == "JPName").Value,
                    KR = general.FirstOrDefault(s => s.Key == "KRName").Value
                },
                BuildTime = t.Value.ParseTimeSpan(),
                BuildType = t.Value.All(c => char.IsDigit(c) || c == ':') ? "" : t.Value,
                Class = general.GetValue("Class"),
                Equipment = ParseEquipment(props["S4: Equipment"]),
                Faction = new Faction {Name = general.GetValue("Nationality"), Prefix = general.GetValue("Nationality").ToShipPrefix()},
                Rarity = rarity,
                Stars = rarity.ToStarRating(),
                Type = general.GetValue("Type"),
                Url = $"{_overrideUrl ?? ConfigConstants.baseUrl}{WikiSearcher.GetPageNameForShipName(shipName)}",
            };
            return new[] {ship};
        }

        private IEnumerable<EquipmentSlot> ParseEquipment(Dictionary<string, string> dict) {
            var slots = dict.GroupBy(kv => kv.Key.FirstOrDefault(char.IsDigit));
            foreach (var slot in slots) {
                yield return new EquipmentSlot {
                    Slot = System.Convert.ToInt32(slot.Key),
                    Type = slot.FirstOrDefault(e => e.Key.EndsWith("Type")).Value,
                    Efficiency = string.Join(" → ", slot.Where(e => e.Key.Contains("Eff")))
                };
            }
        }

        public Task<Ship> GetShipDetails(ShipSummary summary)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<ShipSummary>> GetShipList()
        {
            throw new System.NotImplementedException();
        }
    }
}