using System.Diagnostics;

namespace CourseLibrary.Api.Configuration.Observability.Metrics.Middlewares;

public sealed class RequestMetricsMiddleware(
    RequestDelegate next,
    ILogger<RequestMetricsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startTimestamp = Stopwatch.GetTimestamp();
        Meters.ActiveRequests.Add(1);
        logger.LogDebug(
            "Started handling request: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        try
        {
            await next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            var tags = new TagList
            {
                { "http.request.method", context.Request.Method },
                { "http.response.status_code", context.Response.StatusCode }
            };

            logger.LogDebug(
                "Finished handling request: {Method} {Path} with status code {StatusCode} in {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsed.TotalMilliseconds);

            try
            {
                Meters.RequestCount.Add(1, tags);
                Meters.RequestDuration.Record(
                    elapsed.TotalMilliseconds,
                    tags);

                if (context.Request.ContentLength.HasValue)
                {
                    Meters.RequestBodySize.Record(
                        context.Request.ContentLength.Value,
                        tags);
                }

                if (context.Response.ContentLength.HasValue)
                {
                    Meters.ResponseBodySize.Record(
                        context.Response.ContentLength.Value,
                        tags);
                }

                Meters.ActiveRequests.Add(-1);
            }
            catch (Exception exception)
            {
                logger.LogDebug(
                    exception,
                    "Failed to record request duration metric for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }
        }
    }
}