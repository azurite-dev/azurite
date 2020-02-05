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
            
            return _types.TryGetValue(s, out var prefix) ? prefix : s;
        }

        public static string ExpandPrefix(this string s) {
            var full = _types.FirstOrDefault(kv => kv.Value == s);
            return string.IsNullOrWhiteSpace(full.Key) ? s : full.Key;
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
