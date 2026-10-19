using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Client.Observability;

internal static partial class CourseApiClientLogs
{
    [LoggerMessage(1001, LogLevel.Debug, "Calling Course API operation {Operation}.", EventName = nameof(Calling))]
    public static partial void Calling(this ILogger logger, string operation);

    [LoggerMessage(1002, LogLevel.Warning, "Course API operation {Operation} failed with status {StatusCode}.", EventName = nameof(Failed))]
    public static partial void Failed(this ILogger logger, string operation, int statusCode);

    [LoggerMessage(1003, LogLevel.Error, "Course API operation {Operation} failed unexpectedly.", EventName = nameof(FailedUnexpectedly))]
    public static partial void FailedUnexpectedly(this ILogger logger, Exception exception, string operation);

    [LoggerMessage(1004, LogLevel.Debug, "Course API operation {Operation} completed with status {StatusCode} in {ElapsedMilliseconds} ms.", EventName = nameof(Completed))]
    public static partial void Completed(
        this ILogger logger,
        string operation,
        int statusCode,
        double elapsedMilliseconds);

    [LoggerMessage(1005, LogLevel.Information, "Course API operation {Operation} was cancelled.", EventName = nameof(Cancelled))]
    public static partial void Cancelled(this ILogger logger, string operation);
}
