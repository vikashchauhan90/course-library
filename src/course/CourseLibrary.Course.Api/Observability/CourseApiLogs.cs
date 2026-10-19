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

    [LoggerMessage(
        EventId = 7610,
        Level = LogLevel.Debug,
        Message = "Request context established for request {RequestId}.",
        EventName = nameof(RequestContextEstablished))]
    public static partial void RequestContextEstablished(
        this ILogger logger,
        string requestId);

    [LoggerMessage(
        EventId = 7611,
        Level = LogLevel.Debug,
        Message = "User context added to request telemetry.",
        EventName = nameof(UserContextAdded))]
    public static partial void UserContextAdded(this ILogger logger);

    [LoggerMessage(
        EventId = 7612,
        Level = LogLevel.Debug,
        Message = "Response correlation headers written for request {RequestId}, correlation {CorrelationId}, and trace {TraceId}.",
        EventName = nameof(ResponseHeadersWritten))]
    public static partial void ResponseHeadersWritten(
        this ILogger logger,
        string requestId,
        string correlationId,
        string? traceId);

    [LoggerMessage(
        EventId = 7613,
        Level = LogLevel.Debug,
        Message = "Request {Method} {Path} was cancelled by the client. Trace {TraceId}.",
        EventName = nameof(RequestCancelled))]
    public static partial void RequestCancelled(
        this ILogger logger,
        string traceId,
        string? path,
        string method);

    [LoggerMessage(
        EventId = 7614,
        Level = LogLevel.Error,
        Message = "Request {Method} {Path} failed with status {StatusCode}. Trace {TraceId}.",
        EventName = nameof(UnhandledRequestFailed))]
    public static partial void UnhandledRequestFailed(
        this ILogger logger,
        Exception exception,
        int statusCode,
        string traceId,
        string? path,
        string method);

    [LoggerMessage(
        EventId = 7615,
        Level = LogLevel.Warning,
        Message = "Request {Method} {Path} was rejected with status {StatusCode}. Trace {TraceId}.",
        EventName = nameof(RequestRejected))]
    public static partial void RequestRejected(
        this ILogger logger,
        Exception exception,
        int statusCode,
        string traceId,
        string? path,
        string method);

    [LoggerMessage(
        EventId = 7616,
        Level = LogLevel.Warning,
        Message = "Request failed after the response started. Trace {TraceId}, path {Path}.",
        EventName = nameof(FailureAfterResponseStarted))]
    public static partial void FailureAfterResponseStarted(
        this ILogger logger,
        Exception exception,
        string traceId,
        string? path);

    [LoggerMessage(
        EventId = 7617,
        Level = LogLevel.Debug,
        Message = "Request {Path} was cancelled while writing its error response. Trace {TraceId}.",
        EventName = nameof(ErrorResponseWriteCancelled))]
    public static partial void ErrorResponseWriteCancelled(
        this ILogger logger,
        string traceId,
        string? path);

    [LoggerMessage(
        EventId = 7618,
        Level = LogLevel.Debug,
        Message = "Started recording API request metrics for {Method} {Path}.",
        EventName = nameof(RequestMetricsStarted))]
    public static partial void RequestMetricsStarted(
        this ILogger logger,
        string method,
        string? path);
}
