using System;

namespace Azurite.Wiki
{
    internal static class StatHelpers
    {

        internal static StatisticsSet ToStatistics(this Newtonsoft.Json.Linq.JToken statPackage) {
            var set = new StatisticsSet();
            foreach (var item in statPackage["values"])
            {
                if (item.ParseStat(out (string key, string value) stat)) {
                    set.AddStat(stat.key, stat.value);
                }
                // set.AddStat(item["name"]?.ToString() ?? item["altName"]?.ToString(), item["value"].ToString());
            }
            return set;
        }

        private static bool ParseStat(this Newtonsoft.Json.Linq.JToken item, out (string key, string value) stat) {
            var name = item["name"]?.ToString() ?? item["altName"]?.ToString();
            var value = item["value"]?.ToString();
            stat = (name, value);
            return !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value);
        }
        internal static StatisticsSet AddStat(this StatisticsSet set, (string name, string abbrev) s, string value) {
            switch (s.abbrev)
            {
                case "HP":
                    SetValue(value, i => set.HP = i);
                    return set;
                case "FP":
                    set.Firepower = int.TryParse(value, out int fp) ? fp : 0;
                    return set;
                case "AA":
                    set.AntiAir = int.TryParse(value, out int aa) ? aa : 0;
                    return set;
                case "ASW":
                    set.AntiSub = int.TryParse(value, out int asw) ? asw : 0;
                    return set;
                case "TRP":
                    set.Torpedo = int.TryParse(value, out int trp) ? trp : 0;
                    return set;
                case "AVI":
                    set.Aviation = int.TryParse(value, out int avi) ? avi : 0;
                    return set;
                case "LCK":
                    set.Luck = int.TryParse(value, out int lck) ? lck : 0;
                    return set;
                case "RLD":
                    set.Reload = int.TryParse(value, out int rld) ? rld : 0;
                    return set;
                case "EVA":
                    SetValue(value, eva => set.Evasion = eva);
                    return set;
                case "OIL":
                    SetValue(value, oil => set.OilCost = oil);
                    return set;
                case "SPD":
                    SetValue(value, spd => set.Speed = spd);
                    return set;
                case "HIT":
                    SetValue(value, hit => set.Accuracy = hit);
                    return set;
                case "OXY":
                    SetValue(value, oxy => set.Oxygen = oxy);
                    return set;
                case "AMO":
                    SetValue(value, amo => set.Ammunition = amo);
                    return set;
                case "Armor":
                    set.Armor = Enum.TryParse(value, true, out StatisticsSet.ArmorType type) ? type : StatisticsSet.ArmorType.Light;
                    return set;
                default:
                    return set;
            }
        }

        private static void SetValue(string value, Action<int> setAction) {
            var parsed = int.TryParse(value, out int p) ? p : 0;
            setAction.Invoke(parsed);

        }

        internal static StatisticsSet AddStat(this StatisticsSet set, string s, string value) {
            var parsed = s.ParseStatName();
            return StatHelpers.AddStat(set, parsed, value);
        }

        internal static (string name, string abbreviation) ParseStatName(this string s) {
            switch (s.ToLower().Trim())
            {
                case "health":
                    return ("Health", "HP");
                case "armor":
                    return ("Armor", "Armor");
                case "reload":
                    return ("Reload", "RLD");
                case "luck":
                    return ("Luck", "LCK");
                case "firepower":
                    return ("Firepower", "FP");
                case "torpedo":
                    return ("Torpedo", "TRP");
                case "evasion":
                    return ("Evasion", "EVA");
                case "speed":
                case "spd":
                    return ("Speed", "SPD");
                case "anti-air":
                case "antiair":
                    return ("Anti-air", "AA");
                case "aviation":
                    return ("Aviation", "AVI");
                case "oil":
                case "oil consumption":
                case "cost":
                    return ("Cost", "OIL");
                case "accuracy":
                    return ("Accuracy", "HIT");
                case "anti-submarine warfare":
                    return ("Anti-submarine Warfare", "ASW");
                case "oxygen":
                    return ("Oxygen", "OXY");
                case "ammo":
                case "ammunition":
                    return ("Ammunition", "AMO");
                default:
                    return ("", "");
            }
        }
    }
}