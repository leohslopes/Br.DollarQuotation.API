using System.Diagnostics;

namespace Br.DollarQuotation.API.Middlewares;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;

            if (statusCode >= 500)
            {
                _logger.LogError(
                    "HTTP {Method} {Path} respondeu {StatusCode} em {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    context.TraceIdentifier);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "HTTP {Method} {Path} respondeu {StatusCode} em {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    context.TraceIdentifier);
            }
            else
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path} respondeu {StatusCode} em {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    context.TraceIdentifier);
            }
        }
    }
}