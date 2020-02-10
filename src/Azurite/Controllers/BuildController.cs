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
    [Microsoft.AspNetCore.Mvc.TypeFilter(typeof(IndexCheckAttribute))]
    /// <summary>
    /// Actions related to ship building, including build times and pools.
    /// </summary>
    public class BuildController : ControllerBase
    {
        private readonly ILogger<BuildController> _logger;
        private readonly IShipDataProvider _provider;

        public BuildController(
            ILogger<BuildController> logger,
            IShipDataProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        /// <summary>
        /// Retrieves all ships with a given build time, optionally filtering by pool.
        /// </summary>
        /// <param name="time">The build time when first queued (uses HH:mm:ss form in APIs).</param>
        /// <param name="type">Optionally filter results to ships from a given construction pool (Light, Heavy, Special).</param>
        /// <returns>Summary details of all ships with the given build time and from the specified build pool (where specified).</returns>
        [HttpGet("{time:timespan}")]
        public async Task<IEnumerable<ShipSummary>> GetShipsForBuildTime([FromRoute] TimeSpan time, [FromQuery]ConstructionType? type) {
            var shipDetails = await _provider.GetShipDetails();
            var matches = shipDetails.Where(s => s.BuildTime.HasValue && s.BuildTime.Value == time);
            if (type.HasValue) {
                matches = matches.Where(s => type.Value.ToShipTypes().Any(t => t.Is(s.Type)));
            }
            return matches.Select(ShipSummary.FromShip);
        }

        /// <summary>
        /// Retrieves all ships from a given build pool.
        /// </summary>
        /// <param name="type">The build pool to retrieve ships for (Light, Heavy, Special)</param>
        /// <returns>Summary details of all ships from the given build pool.</returns>
        [HttpGet("{type}")]
        public async Task<IEnumerable<Ship>> GetShipsForBuildPool([FromRoute]ConstructionType type) {
            var shipDetails = await _provider.GetShipDetails();
            var classesForPool = type.ToShipTypes();
            var ships = shipDetails.Where(sd => sd.BuildTime.HasValue).Where(s => classesForPool.Any(t => t.Is(s.Type)));
            return ships;
        }
    }
}