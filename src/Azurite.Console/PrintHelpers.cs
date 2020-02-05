using System;
using System.Collections.Generic;
using ConsoleTables;

namespace Azurite.Console
{
    public static class PrintHelpers
    {
        internal static void PrintJson<TObject>(TObject o) {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented);
            System.Console.WriteLine(json);
        }
        internal static int PrintShipList(ListCommandSettings settings, IEnumerable<Ship> shipList) {
            switch (settings.Output)
            {
                case ListOutput.List:
                    foreach (var ship in shipList)
                    {
                        System.Console.WriteLine(ship.ToString());
                    }
                    return 200;
                case ListOutput.Table:
                    var table = new ConsoleTable("ID", "Ship", "Type", "Class", "Faction", "Rarity");
                    foreach (var ship in shipList)
                    {
                        table.AddRow(ship.ShipId, ship.ShipName.EN, ship.Type, ship.Class, ship.Faction.Name, ship.Rarity);
                    }
                    table.Write(Format.Minimal);
                    return 200;
                case ListOutput.Json:
                    PrintJson(shipList);
                    return 200;
                default:
                    return 400;
            }
        }

        internal static int PrintSingleShip(Ship ship, bool allNames = false) {
            var info = new ConsoleTable("ID", "Name", "Type", "Class", "Faction");
            info.AddRow(ship.ShipId, ship.ToString(false), ship.Type, ship.Class, ship.Faction.Name);
            info.Write(Format.Minimal);
            if (allNames) {
                new ConsoleTable("EN", "JP", "CN", "KR")
                    .AddRow(ship.ShipName.EN, ship.ShipName.JP, ship.ShipName.CN, ship.ShipName.KR)
                    .Write(Format.Minimal);
            }
            info = new ConsoleTable("Rarity", "Stars", "Build Time", "Source");
            var stars = $"{new String('★', ship.Stars.Default)}{new String('☆', ship.Stars.Max - ship.Stars.Default)}";
            info.AddRow(ship.Rarity, stars, ship.BuildTime.HasValue ? ship.BuildTime.Value.ToString() : "N/A", ship.BuildType ?? "Construction");
            info.Write(Format.Minimal);
            info = new ConsoleTable("Slot", "Efficiency", "Equipment");
            foreach (var slot in ship.Equipment)
            {
                info.AddRow(slot.Slot, slot.Efficiency, slot.Type);
            }
            info.Write(Format.Minimal);
            System.Console.WriteLine($"Full ship details available at {ship}");
            return 0;
        }
    }
}