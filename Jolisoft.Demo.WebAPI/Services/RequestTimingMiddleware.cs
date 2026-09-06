using System.Diagnostics;

namespace Jolisoft.Demo.WebAPI.Services;

public sealed class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();
        await next(context);
        timer.Stop();
        logger.LogInformation("{Method} {Path} returned {StatusCode} in {ElapsedMs}ms", context.Request.Method, context.Request.Path, context.Response.StatusCode, timer.ElapsedMilliseconds);
    }
}