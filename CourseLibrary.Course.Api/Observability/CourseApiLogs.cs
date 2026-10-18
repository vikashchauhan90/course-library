using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Api.Observability;

internal static partial class CourseApiLogs
{
    [LoggerMessage(
        EventId = 7601,
        Level = LogLevel.Information,
        Message = "API endpoint {EndpointName} completed {Method} request with status {StatusCode} and outcome {Outcome} in {DurationMilliseconds} ms.",
        EventName = nameof(EndpointRequestCompleted))]
    public static partial void EndpointRequestCompleted(
        this ILogger logger,
        string endpointName,
        string method,
        int? statusCode,
        string outcome,
        double durationMilliseconds);

    [LoggerMessage(
        EventId = 7602,
        Level = LogLevel.Warning,
        Message = "API endpoint {EndpointName} cancelled {Method} request after {DurationMilliseconds} ms.",
        EventName = nameof(EndpointRequestCancelled))]
    public static partial void EndpointRequestCancelled(
        this ILogger logger,
        string endpointName,
        string method,
        double durationMilliseconds);

    [LoggerMessage(
        EventId = 7603,
        Level = LogLevel.Error,
        Message = "API endpoint {EndpointName} failed {Method} request after {DurationMilliseconds} ms.",
        EventName = nameof(EndpointRequestFailed))]
    public static partial void EndpointRequestFailed(
        this ILogger logger,
        Exception exception,
        string endpointName,
        string method,
        double durationMilliseconds);
}
