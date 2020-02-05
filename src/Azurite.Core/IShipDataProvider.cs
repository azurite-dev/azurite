using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azurite
{
    public interface IShipDataProvider
    {
        Task<List<ShipSummary>> GetShipList();
        Task<IEnumerable<Ship>> GetShipDetails(string shipName);
        Task<Ship> GetShipDetails(ShipSummary summary);        
    }
}