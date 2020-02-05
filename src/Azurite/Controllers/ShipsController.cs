using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azurite.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Azurite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [TypeFilter(typeof(IndexCheckAttribute))]
    public class ShipsController : ControllerBase
    {
        private readonly ILogger<ShipsController> _logger;
        private readonly IShipDataProvider _provider;

        public ShipsController(
            ILogger<ShipsController> logger,
            IShipDataProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        [HttpGet]
        public async Task<IEnumerable<ShipSummary>> GetShips(
            [FromQuery]string name = null, 
            [FromQuery]string type = null, 
            [FromQuery]string faction = null,
            [FromQuery]string rarity = null
        )
        {
            // var className = @class ?? string.Empty;
            var ships = (await _provider.GetShipList())
                .Where(s => string.IsNullOrWhiteSpace(name) ? true : s.Name.Is(name))
                .Where(s => string.IsNullOrWhiteSpace(type) ? true : s.Type.Is(type))
                .Where(s => string.IsNullOrWhiteSpace(faction) ? true : s.FactionName.Is(faction))
                .Where(s => string.IsNullOrWhiteSpace(rarity) ? true : s.Rarity.Is(rarity)) //TODO: WEAK. do better.
            ;

            return ships;
        }

        
        [HttpGet("{shipId}")]
        public async Task<Ship> GetShipById([FromRoute]string shipId) {
            //TODO: improve this.
            var ship = await _provider.GetShipDetails(new ShipSummary { Id = shipId});
            return ship;
        }

        [HttpGet("class/{class}")]
        public async Task<IEnumerable<ShipSummary>> GetShipsForClass([FromRoute]string @class) {
            var className = @class;
            var ships = await _provider.GetShipDetails();
            return ships.Where(s => s.Class.Is(className)).Select(ShipSummary.FromShip);
        }
    }
}
