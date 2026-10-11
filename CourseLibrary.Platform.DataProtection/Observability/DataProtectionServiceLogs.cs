using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.DataProtection.Observability;

internal static partial class DataProtectionServiceLogs
{
    [LoggerMessage(5201, LogLevel.Debug, "Data protection {Operation} completed for {PayloadType} payload.")]
    public static partial void OperationSucceeded(
        this ILogger logger,
        string operation,
        string payloadType);

    [LoggerMessage(5202, LogLevel.Error, "Data protection {Operation} failed for {PayloadType} payload.")]
    public static partial void OperationFailed(
        this ILogger logger,
        Exception exception,
        string operation,
        string payloadType);
}