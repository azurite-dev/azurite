using System;

namespace Azurite
{
    public class ShipSummary {
        public string Id {get;set;}
        public string Name {get;set;}
        public string Rarity {get;set;}
        public string Type {get;set;}
        //[Obsolete]
        //public string Subtype {get;set;}
        public string FactionName {get;set;}

        public static ShipSummary FromShip(Ship ship) {
            return new ShipSummary {
                Id = ship.ShipId,
                Name = ship.ShipName,
                Rarity = ship.Rarity.ToString(),
                Type = ship.Type,
                // Subtype = ship.Subtype ?? string.Empty,
                FactionName = ship.Faction?.Name ?? string.Empty
            };
        }
    }
}
