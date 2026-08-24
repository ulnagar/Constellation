namespace Constellation.Infrastructure.Middleware;

using Application.Interfaces.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public class ScannerBlocklistMiddleware
{
    private readonly RequestDelegate _next;

    public ScannerBlocklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context, 
        IScannerPathMatcherService matcher, 
        ILogger logger)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (matcher.IsBlockedPath(path))
        {
            logger
                .ForContext<ScannerBlocklistMiddleware>()
                .Debug("Blocked scanner-pattern request: {Path} from {RemoteIp}",
                path, context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 404; // pull from options if you want it configurable per-response
            return; // short-circuits — never reaches routing, auth, or your error handler
        }

        await _next(context);
    }
}

public static class ScannerBlocklistMiddlewareExtensions
{
    public static IApplicationBuilder UseScannerBlocklist(this IApplicationBuilder app)
        => app.UseMiddleware<ScannerBlocklistMiddleware>();
}