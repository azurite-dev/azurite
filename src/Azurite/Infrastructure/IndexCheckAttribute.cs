using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azurite.Infrastructure
{
    public class IndexCheckAttribute : ActionFilterAttribute
    {
        private readonly bool _skip;
        private readonly IShipDataProvider _provider;
        private readonly ILogger<IndexCheckAttribute> _logger;

        public IndexCheckAttribute(IShipDataProvider provider, IOptions<AzuriteOptions> options, ILogger<IndexCheckAttribute> logger)
        {
            _skip = options.Value.FailOnEmptyIndex;
            _provider = provider;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            _logger.LogTrace($"Running {nameof(IndexCheckAttribute)} for request.");
            var ships = _provider.GetShipList().Result.Any();
            if (!ships) {
                _logger.LogWarning($"Executing {context.RouteData.ToString()} failed as no results were returned from data provider!");
                context.Result = new IndexFailedResult();
            }
        }
    }
}