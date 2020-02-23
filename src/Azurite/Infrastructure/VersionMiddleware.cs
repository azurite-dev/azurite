using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Azurite.Infrastructure
{
    public class VersionMiddleware
{
    readonly RequestDelegate _next;
    static readonly Assembly _entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
    static readonly string _version = System.Diagnostics.FileVersionInfo.GetVersionInfo(_entryAssembly.Location).FileVersion;

    public VersionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync(_version);

        //we're all done, so don't invoke next middleware
    }
}
}