using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Idempotency.Observability;

internal static partial class IdempotencyStoreLogs
{
    [LoggerMessage(5401, LogLevel.Debug, "Idempotency lookup completed with outcome {Outcome}.")]
    public static partial void LookupCompleted(this ILogger logger, string outcome);

    [LoggerMessage(5402, LogLevel.Debug, "Idempotency acquisition completed with outcome {Outcome}.")]
    public static partial void AcquisitionCompleted(this ILogger logger, string outcome);

    [LoggerMessage(5403, LogLevel.Debug, "Idempotency entry stored with status {Status}.")]
    public static partial void EntryStored(this ILogger logger, string status);

    [LoggerMessage(5404, LogLevel.Debug, "Idempotency entry removed.")]
    public static partial void EntryRemoved(this ILogger logger);

    [LoggerMessage(5405, LogLevel.Debug, "Idempotency {Operation} operation was cancelled.")]
    public static partial void OperationCancelled(this ILogger logger, string operation);

    [LoggerMessage(5406, LogLevel.Error, "Idempotency {Operation} operation failed.")]
    public static partial void OperationFailed(
        this ILogger logger,
        Exception exception,
        string operation);
}