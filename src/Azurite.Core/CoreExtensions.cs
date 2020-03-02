using System.Collections.Generic;
using System.Linq;

namespace Azurite
{
    public static class CoreExtensions {
        private static Dictionary<string, string> _types = new Dictionary<string, string>{
                ["Destroyer"] = "DD",
                ["Light Cruiser"] = "CL",
                ["Heavy Cruiser"] = "CA",
                ["Battleship"] = "BB",
                ["Monitor"] = "BM",
                ["Battle Cruiser"] = "BC",
                ["Aviation Battleship"] = "BBV",
                ["Aircraft Carrier"] = "CV",
                ["Light Aircraft Carrier"] = "CVL",
                ["Submarine"] = "SS",
                ["Repair Ship"] = "AR"
            };
        public static string ToWikiName(this string name) {
            return name.Trim().Replace(' ', '_');
        }
        internal static string ToPrefix(this string s) {
            s = s.ToTitleCase();
            return _types.TryGetValue(s, out var prefix) ? prefix : s;
        }

        public static string ExpandPrefix(this string s) {
            var full = _types.FirstOrDefault(kv => kv.Value == s);
            return string.IsNullOrWhiteSpace(full.Key) ? s : full.Key;
        }

        public static string ToShipPrefix(this string factionName) {
            var dict = new Dictionary<string, string> {
                ["Eagle Union"] = "USS",
                ["Iris Libre"] = "FFNF",
                ["Eastern Radiance"] = "PRAN",
                ["Ironblood"] = "KMS",
                ["Neptunia"] = "HDN",
                ["North Union"] = "SN",
                ["Royal Navy"] = "HMS",
                ["Sakura Empire"] = "IJN",
                ["Universal"] = "UNIV",
                ["Vichya Dominion"] = "MNF"
            };
            return dict[factionName];
        }

        public static Faction SetPrefix(this Faction f, string overridePrefix = null) {
            f.Prefix = overridePrefix ?? f.Name.ToShipPrefix();
            return f;
        }

        public static Stars ToStarRating(this Rarity rarity) {
            switch (rarity)
            {
                case Rarity.Common:
                    return new Stars {Default = 1, Max = 4};
                case Rarity.Rare:
                case Rarity.Elite:
                    return new Stars { Default = 2, Max = 5};
                case Rarity.SuperRare:
                case Rarity.UltraRare:
                case Rarity.Decisive:
                    return new Stars { Default = 3, Max = 6};
                default:
                    return null;
            }
        }

        public static string ToTitleCase(this string s) {
            return new System.Globalization.CultureInfo("en-US", true).TextInfo.ToTitleCase(s);
        }
    }

    public static class Helpers {
        public static bool IsRetrofit(string id) {
            return int.TryParse(id, out var intId) && intId > 3000;
        }

        public static bool IsRetrofit(ShipSummary summary) {
            return IsRetrofit(summary.Id);
        }

        public static bool IsRetrofit(Ship ship) {
            return IsRetrofit(ship.ShipId);
        }

        public static Rarity ParseRarity(string s) {
            if (string.IsNullOrWhiteSpace(s)) return Rarity.None;
            if (s.Trim().Equals("normal", System.StringComparison.InvariantCultureIgnoreCase)) return Rarity.Common;
            return System.Enum.TryParse(s.Trim().Replace(" ", ""), true, out Rarity rarity) ? rarity : Rarity.None;
        }

        public static bool TryParseRarity(string s, out Rarity? rarity) {
            var parsed = ParseRarity(s);
            rarity = parsed;
            if (parsed == Rarity.None) {
                return false;
            }
            return true;
        }
    }
}
