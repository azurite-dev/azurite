using System;
using System.Reflection;
using System.Threading.Tasks;
using Azurite.Index;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Azurite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InfoController : ControllerBase
    {
        private readonly IndexBuilder _builder;
        private static Task _rebuild;

        public InfoController(Index.IndexBuilder builder, IOptions<AspNetCoreRateLimit.IpRateLimitOptions> opts)
        {
            _builder = builder;
            _builder.LogToConsole = true;
        }
        [HttpGet]
        public IActionResult GetAppInfo() {
            return new ObjectResult(new {
                time = DateTime.UtcNow
            });
        }

        [HttpPost("index")]
        [TypeFilter(typeof(Infrastructure.IndexRebuildAccessFilter))]
        public IActionResult RebuildIndex() {
            if (_rebuild != null && !_rebuild.IsCompleted) return new StatusCodeResult(423);
            _rebuild = Task.Run(() => _builder.BuildShipIndex(2500, IndexBuildOptions.SideBySide));
            return new AcceptedResult();
        }
    }
}