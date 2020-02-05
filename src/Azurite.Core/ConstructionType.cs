using System;
using System.Collections.Generic;
using System.Linq;

namespace Azurite
{
    [Flags]
    public enum ConstructionType
    {
        Light = 0,
        Heavy = 1,
        Special = 2
    }

    public static class ConstructionTypeExtensions {
        public static IEnumerable<string> ToShipTypes(this ConstructionType c, bool usePrefix = false) {
            IEnumerable<string> types;
            switch (c)
            {
                case ConstructionType.Light:
                    types = new[] { "Light Cruiser", "Light Aircraft Carrier", "Destroyer", "Repair Ship"};
                    break;
                case ConstructionType.Heavy:
                    types = new[] { "Light Cruiser", "Heavy Cruiser", "Monitor", "Battle Cruiser", "Battleship"};
                    break;
                case ConstructionType.Special:
                    types = new[] { "Aircraft Carrier", "Light Aircraft Carrier", "Light Cruiser", "Heavy Cruiser", "Repair Ship", "Submarine"};
                    break;
                default:
                    types = new List<string>();
                    break;
            }
            return usePrefix
                ? types.Select(s => s.ToPrefix())
                : types;
        }
    }
}