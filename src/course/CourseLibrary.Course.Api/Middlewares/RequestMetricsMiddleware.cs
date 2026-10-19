using CourseLibrary.Course.Api.Observability;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics;

namespace CourseLibrary.Course.Api.Middlewares;

internal sealed class RequestMetricsMiddleware(
    RequestDelegate next,
    ILogger<RequestMetricsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var method = context.Request.Method;
        CourseApiMeter.Shared.RequestStarted();

        var outcome = "success";
        try
        {
            logger.RequestMetricsStarted(method, context.Request.Path.Value);
            await next(context);
            outcome = GetOutcome(context.Response.StatusCode);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            outcome = "cancelled";
            throw;
        }
        catch
        {
            outcome = "failure";
            throw;
        }
        finally
        {
            try
            {
                var endpointName = GetEndpointName(context);
                var statusCode = context.Response.StatusCode;
                var durationMilliseconds = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
                if (context.Request.ContentLength is long requestSize)
                {
                    Activity.Current?.SetTag(
                        CourseApiActivityTagConstants.RequestBodySize,
                        requestSize);
                }

                if (context.Response.ContentLength is long responseSize)
                {
                    Activity.Current?.SetTag(
                        CourseApiActivityTagConstants.ResponseBodySize,
                        responseSize);
                }

                CourseApiMeter.Shared.RecordRequest(
                    endpointName,
                    method,
                    GetOutcome(statusCode, outcome),
                    durationMilliseconds,
                    context.Request.ContentLength,
                    context.Response.ContentLength);
            }
            finally
            {
                CourseApiMeter.Shared.RequestCompleted();
            }
        }
    }

    private static string GetEndpointName(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName
            ?? (endpoint as RouteEndpoint)?.RoutePattern.RawText
            ?? endpoint?.DisplayName
            ?? "unmatched";
    }

    private static string GetOutcome(int statusCode)
        => statusCode switch
        {
            >= StatusCodes.Status500InternalServerError => "server_error",
            >= StatusCodes.Status400BadRequest => "client_error",
            _ => "success"
        };

    private static string GetOutcome(int statusCode, string requestOutcome)
        => requestOutcome is "cancelled" or "failure"
            ? requestOutcome
            : GetOutcome(statusCode);
}
