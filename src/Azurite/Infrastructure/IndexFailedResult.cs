using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Azurite.Infrastructure
{
    public class IndexFailedResult : IActionResult
    {
        private readonly object _message;
        private readonly string _defaultMessage = "The configured provider returned no results! The index may not be populated. Cannot continue.";

        public IndexFailedResult(string message = null)
        {
            _message = message ?? _defaultMessage;
        }
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var objResult = new ObjectResult(_message) {
                StatusCode = 507
            };
            await objResult.ExecuteResultAsync(context);
        }
    }
}