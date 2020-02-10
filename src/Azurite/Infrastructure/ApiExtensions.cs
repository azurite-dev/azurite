using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azurite.Infrastructure
{
    public static class ApiExtensions
    {
        internal static bool Is(this string s, string value) {
            return !string.IsNullOrWhiteSpace(s) && s.Trim().Equals(value.Trim(), System.StringComparison.InvariantCultureIgnoreCase);
        }

        internal static async Task<IEnumerable<Ship>> GetShipDetails(this IShipDataProvider _provider) {
            var allShips = await _provider.GetShipList();
            var ships = new List<Ship>();
            foreach (var ship in allShips)
            {
                ships.Add(await _provider.GetShipDetails(ship));
            }
            return ships;
        }
    }
}