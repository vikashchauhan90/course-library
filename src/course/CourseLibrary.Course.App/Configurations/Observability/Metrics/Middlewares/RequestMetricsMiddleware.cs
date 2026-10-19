using System.Diagnostics;
using CourseLibrary.Course.App.Configuration.Observability.Traces;
using Microsoft.AspNetCore.Routing;

namespace CourseLibrary.Course.App.Configuration.Observability.Metrics.Middlewares;

public sealed class RequestMetricsMiddleware(
    RequestDelegate next,
    ILogger<RequestMetricsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpointName = context.GetEndpoint()?
            .Metadata
            .GetMetadata<IEndpointNameMetadata>()?
            .EndpointName
            ?? (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText
            ?? "unmatched";
        var method = context.Request.Method;
        var startTimestamp = Stopwatch.GetTimestamp();
        using var activity = ActivitySources.App.StartActivity(
            $"app.endpoint.{endpointName}");
        activity?.SetTag("app.endpoint.name", endpointName);
        activity?.SetTag("http.request.method", method);

        Meters.ActiveRequests.Add(1);
        logger.LogDebug(
            "Started handling request: {Method} {Path}",
            method,
            context.Request.Path);

        var outcome = "success";
        Exception? requestException = null;
        try
        {
            await next(context);
        }
        catch (OperationCanceledException exception) when (context.RequestAborted.IsCancellationRequested)
        {
            outcome = "cancelled";
            requestException = exception;
            throw;
        }
        catch (Exception exception)
        {
            outcome = "failure";
            requestException = exception;
            throw;
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            var statusCode = context.Response.StatusCode;
            if (requestException is null)
            {
                outcome = GetOutcome(statusCode);
            }

            var tags = new TagList
            {
                { "app.endpoint.name", endpointName },
                { "app.endpoint.outcome", outcome },
                { "http.request.method", method },
                { "http.response.status_code", statusCode }
            };

            activity?.SetTag("app.endpoint.outcome", outcome);
            activity?.SetTag("app.endpoint.duration_ms", elapsed.TotalMilliseconds);
            activity?.SetTag("http.response.status_code", statusCode);

            if (requestException is not null)
            {
                activity?.SetTag("error.type", requestException.GetType().Name);
                activity?.SetStatus(
                    ActivityStatusCode.Error,
                    outcome == "cancelled" ? "Request cancelled." : "Request failed.");
                if (outcome == "cancelled")
                {
                    logger.LogWarning(
                        "UI endpoint {EndpointName} cancelled {Method} request after {DurationMilliseconds} ms.",
                        endpointName,
                        method,
                        elapsed.TotalMilliseconds);
                }
                else
                {
                    logger.LogError(
                        requestException,
                        "UI endpoint {EndpointName} failed {Method} request after {DurationMilliseconds} ms.",
                        endpointName,
                        method,
                        elapsed.TotalMilliseconds);
                }
            }
            else
            {
                activity?.SetStatus(
                    statusCode >= StatusCodes.Status500InternalServerError
                        ? ActivityStatusCode.Error
                        : ActivityStatusCode.Ok);
                logger.LogInformation(
                    "UI endpoint {EndpointName} completed {Method} request with status {StatusCode} and outcome {Outcome} in {DurationMilliseconds} ms.",
                    endpointName,
                    method,
                    statusCode,
                    outcome,
                    elapsed.TotalMilliseconds);
            }

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
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Failed to record UI request metrics for {EndpointName} {Method}.",
                    endpointName,
                    method);
            }
            finally
            {
                Meters.ActiveRequests.Add(-1);
            }
        }
    }

    private static string GetOutcome(int statusCode)
        => statusCode switch
        {
            >= StatusCodes.Status500InternalServerError => "server_error",
            >= StatusCodes.Status400BadRequest => "client_error",
            _ => "success"
        };
}