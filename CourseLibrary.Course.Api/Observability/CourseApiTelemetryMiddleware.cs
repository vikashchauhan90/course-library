using System.Diagnostics;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Api.Observability;

internal sealed class CourseApiTelemetryMiddleware(
    RequestDelegate next,
    ILogger<CourseApiTelemetryMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var endpointName = httpContext.GetEndpoint()?
            .Metadata
            .GetMetadata<IEndpointNameMetadata>()?
            .EndpointName;

        if (string.IsNullOrWhiteSpace(endpointName))
        {
            await next(httpContext);
            return;
        }

        var method = httpContext.Request.Method;
        var startedAt = Stopwatch.GetTimestamp();
        using var activity = CourseApiActivitySource.Shared.StartActivity(
            $"api.endpoint.{endpointName}");
        activity?.SetTag(CourseApiActivityTagConstants.EndpointName, endpointName);
        activity?.SetTag(CourseApiActivityTagConstants.HttpRequestMethod, method);

        try
        {
            await next(httpContext);

            var durationMilliseconds = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            var statusCode = httpContext.Response.StatusCode;
            var outcome = GetOutcome(statusCode);

            activity?.SetTag(CourseApiActivityTagConstants.EndpointOutcome, outcome);
            activity?.SetTag(CourseApiActivityTagConstants.DurationMilliseconds, durationMilliseconds);
            activity?.SetTag(CourseApiActivityTagConstants.HttpResponseStatusCode, statusCode);
            activity?.SetStatus(
                statusCode >= StatusCodes.Status500InternalServerError
                    ? ActivityStatusCode.Error
                    : ActivityStatusCode.Ok);

            CourseApiMeter.Shared.RecordRequest(
                endpointName,
                method,
                outcome,
                durationMilliseconds);
            logger.EndpointRequestCompleted(
                endpointName,
                method,
                statusCode,
                outcome,
                durationMilliseconds);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            var durationMilliseconds = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            activity?.SetTag(CourseApiActivityTagConstants.EndpointOutcome, "cancelled");
            activity?.SetTag(CourseApiActivityTagConstants.DurationMilliseconds, durationMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Error, "Request cancelled.");
            CourseApiMeter.Shared.RecordRequest(
                endpointName,
                method,
                "cancelled",
                durationMilliseconds);
            logger.EndpointRequestCancelled(endpointName, method, durationMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            var durationMilliseconds = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            activity?.SetTag(CourseApiActivityTagConstants.EndpointOutcome, "failure");
            activity?.SetTag(CourseApiActivityTagConstants.ErrorType, exception.GetType().Name);
            activity?.SetTag(CourseApiActivityTagConstants.DurationMilliseconds, durationMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            CourseApiMeter.Shared.RecordRequest(
                endpointName,
                method,
                "failure",
                durationMilliseconds);
            logger.EndpointRequestFailed(
                exception,
                endpointName,
                method,
                durationMilliseconds);
            throw;
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
