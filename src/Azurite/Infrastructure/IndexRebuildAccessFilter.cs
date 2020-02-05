using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azurite.Infrastructure
{
    public class IndexRebuildAccessFilter : ActionFilterAttribute
    {
        private readonly AzuriteOptions _options;
        private readonly IShipDataProvider _provider;
        private readonly ILogger<IndexRebuildAccessFilter> _logger;

        public IndexRebuildAccessFilter(IShipDataProvider provider, IOptions<AzuriteOptions> options, ILogger<IndexRebuildAccessFilter> logger)
        {
            _options = options.Value;
            _provider = provider;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            _logger.LogTrace($"Running {nameof(IndexRebuildAccessFilter)} for request.");
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress;
            var ipList = _options.RebuildWhitelist;
            var isAllowed = (ipList == null || !ipList.Any()) 
                ? false
                : ipList
                    .Select(i => IPAddress.Parse(i))
                    .Any(ip => ip.Equals(ipAddress.MapToIPv4()));
            if (!isAllowed) {
                _logger.LogWarning("Could not allow index function as client IP is not in whitelist!");
                context.Result = new UnauthorizedResult();
            }
        }
    }
}