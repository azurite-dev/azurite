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
    [Microsoft.AspNetCore.Mvc.TypeFilter(typeof(IndexCheckAttribute))]
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

        [HttpGet("{time:timespan}")]
        public async Task<IEnumerable<ShipSummary>> GetShipsForBuildTime([FromRoute] TimeSpan time, [FromQuery]ConstructionType? type) {
            var shipDetails = await _provider.GetShipDetails();
            var matches = shipDetails.Where(s => s.BuildTime.HasValue && s.BuildTime.Value == time);
            if (type.HasValue) {
                matches = matches.Where(s => type.Value.ToShipTypes().Any(t => t.Is(s.Type)));
            }
            return matches.Select(ShipSummary.FromShip);
        }

        [HttpGet("{type}")]
        public async Task<IEnumerable<Ship>> GetShipsForBuildPool([FromRoute]ConstructionType type) {
            var shipDetails = await _provider.GetShipDetails();
            var classesForPool = type.ToShipTypes();
            var ships = shipDetails.Where(sd => sd.BuildTime.HasValue).Where(s => classesForPool.Any(t => t.Is(s.Type)));
            return ships;
        }
    }
}