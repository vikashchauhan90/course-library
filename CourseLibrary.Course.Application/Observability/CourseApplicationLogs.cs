using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Observability;

internal static partial class CourseApplicationLogs
{
    [LoggerMessage(7001, LogLevel.Debug, "Application operation {Operation} started.", EventName = nameof(OperationStarted))]
    public static partial void OperationStarted(this ILogger logger, string operation);

    [LoggerMessage(7002, LogLevel.Debug, "Application operation {Operation} completed in {ElapsedMilliseconds} ms.", EventName = nameof(OperationCompleted))]
    public static partial void OperationCompleted(this ILogger logger, string operation, double elapsedMilliseconds);

    [LoggerMessage(7003, LogLevel.Warning, "Application operation {Operation} was cancelled after {ElapsedMilliseconds} ms.", EventName = nameof(OperationCancelled))]
    public static partial void OperationCancelled(this ILogger logger, string operation, double elapsedMilliseconds);

    [LoggerMessage(7004, LogLevel.Error, "Application operation {Operation} failed after {ElapsedMilliseconds} ms.", EventName = nameof(OperationFailed))]
    public static partial void OperationFailed(
        this ILogger logger,
        Exception exception,
        string operation,
        double elapsedMilliseconds);

    [LoggerMessage(7005, LogLevel.Warning, "Application operation {Operation} was rejected with error {ErrorCode} after {ElapsedMilliseconds} ms.", EventName = nameof(OperationRejected))]
    public static partial void OperationRejected(
        this ILogger logger,
        string operation,
        string errorCode,
        double elapsedMilliseconds);
}
