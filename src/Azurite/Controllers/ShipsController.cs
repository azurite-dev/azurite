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
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [TypeFilter(typeof(IndexCheckAttribute))]
    /// <summary>
    /// Actions relating to ships and ship data, including listing and querying ships.
    /// </summary>
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

        /// <summary>
        /// Gets a summary listing of all ships, optionally filtering by basic ship information.
        /// </summary>
        /// <param name="name">Optional ship name to filter results by. Will only return exact matches.</param>
        /// <param name="type">The hull type to filter by. Supports prefix forms (i.e. DD, CL etc)</param>
        /// <param name="faction">Faction/Nation to filter by. Will only return exact matches, does not support prefixes.</param>
        /// <param name="rarity">Rarity to match by. Do not include spaces! (i.e. "SuperRare", not "Super Rare").</param>
        /// <returns>A collection of matching ships' basic information.</returns>
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
                .Where(s => string.IsNullOrWhiteSpace(type) ? true : (s.Type.Is(type) || s.Type.Is(type.ExpandPrefix())))
                .Where(s => string.IsNullOrWhiteSpace(faction) ? true : s.FactionName.Is(faction))
                .Where(s => string.IsNullOrWhiteSpace(rarity) ? true : s.Rarity.Is(rarity)) //TODO: WEAK. do better.
            ;

            return ships;
        }

        /// <summary>
        /// Gets a specific ship's details by its game ID.
        /// </summary>
        /// <param name="shipId">The Ship ID (not name!) to get details for.</param>
        /// <returns>The full details of the requested ship.</returns>
        [HttpGet("{shipId}")]
        public async Task<Ship> GetShipById([FromRoute]string shipId) {
            //TODO: improve this.
            var ship = await _provider.GetShipDetails(new ShipSummary { Id = shipId});
            return ship;
        }

        /// <summary>
        /// Gets a summary of all ships from a given class.
        /// </summary>
        /// <remarks>
        /// This is not actually exposed in-game, but this makes it much easier to find matching ships for perks.
        /// </remarks>
        /// <param name="class">The name of the ship class to get.</param>
        [HttpGet("class/{class}")]
        public async Task<IEnumerable<ShipSummary>> GetShipsForClass([FromRoute]string @class) {
            var className = @class;
            var ships = await _provider.GetShipDetails();
            return ships.Where(s => s.Class.Is(className)).Select(ShipSummary.FromShip);
        }
    }
}
