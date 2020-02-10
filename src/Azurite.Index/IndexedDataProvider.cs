using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azurite.Index
{
    public class IndexedDataProvider : IShipDataProvider
    {
        private readonly ShipDbClient _client;

        public IndexedDataProvider(ShipDbClient client)
        {
            _client = client;
        }

        public bool IsLocal => true;

        public Task<IEnumerable<Ship>> GetShipDetails(string shipName)
        {
            // var collection = _client.GetShipCollection().FindAll().ToList();
            return Task.FromResult(_client.GetShipCollection().Find(s => s.ShipName.AnyNameIs(shipName)));
        }

        public Task<Ship> GetShipDetails(ShipSummary summary)
        {
            return Task.FromResult(_client.GetShipCollection().FindById(summary.Id));
        }

        public Task<List<ShipSummary>> GetShipList()
        {
            return Task.FromResult(_client.GetShipCollection().FindAll().Select(ShipSummary.FromShip).ToList());
        }
    }
}
