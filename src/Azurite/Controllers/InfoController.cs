using System;
using System.Reflection;
using System.Threading.Tasks;
using Azurite.Index;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Azurite.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    // [Route("[controller]")]
    [Route("api/v{version:apiVersion}")]
    public class InfoController : ControllerBase
    {
        private readonly IndexBuilder _builder;
        private static Task _rebuild;

        public InfoController(Index.IndexBuilder builder, IOptions<AspNetCoreRateLimit.IpRateLimitOptions> opts)
        {
            _builder = builder;
            _builder.LogToConsole = true;
        }

        /// <summary>
        /// Retrieves basic app information.
        /// </summary>
        /// <remarks>
        /// In general, this endpoint shouldn't be needed by consuming applications. 
        /// It is provided as a convenience in case of future compatibility issues.
        /// </remarks>
        /// <returns></returns>
        [HttpGet("/info")]
        [ApiVersionNeutral]
        public IActionResult GetAppInfo() {
            return new ObjectResult(new {
                time = DateTime.UtcNow,
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            });
        }

        /// <summary>
        /// Invokes an index rebuild operation.
        /// </summary>
        /// <remarks>
        /// Given the massive load this can invoke (and misconfiguration risks), by default this request 
        /// can only be invoked from 'localhost' (i.e. the system the API is running on). This is controlled 
        /// by the `Azurite.RebuildWhitelist` config option. This operation takes a *very* long time.
        /// </remarks>
        /// <returns>Status of the build request. 202 if successful, 423 if already in progress.</returns>
        [HttpPost("index")]
        [TypeFilter(typeof(Infrastructure.IndexRebuildAccessFilter))]
        public IActionResult RebuildIndex() {
            if (_rebuild != null && !_rebuild.IsCompleted) return new StatusCodeResult(423);
            _rebuild = Task.Run(() => _builder.BuildShipIndex(2500, IndexBuildOptions.SideBySide));
            return new AcceptedResult();
        }
    }
}