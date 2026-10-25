using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Persistence.Observability;

internal static partial class CosmosRepositoryLogs
{
    [LoggerMessage(
        EventId = 7201,
        Level = LogLevel.Debug,
        Message = "Cosmos operation completed. Operation: {Operation}, Container: {ContainerName}, RequestCharge: {RequestCharge}",
        EventName = nameof(OperationCompleted))]
    public static partial void OperationCompleted(
        this ILogger logger,
        string operation,
        string containerName,
        double requestCharge);

    [LoggerMessage(
        EventId = 7202,
        Level = LogLevel.Debug,
        Message = "Cosmos document was not found. Operation: {Operation}, Container: {ContainerName}, Id: {Id}",
        EventName = nameof(DocumentNotFound))]
    public static partial void DocumentNotFound(
        this ILogger logger,
        string operation,
        string containerName,
        string id);

    [LoggerMessage(
        EventId = 7203,
        Level = LogLevel.Warning,
        Message = "Cosmos operation failed. Operation: {Operation}, Container: {ContainerName}, StatusCode: {StatusCode}, ActivityId: {ActivityId}, RequestCharge: {RequestCharge}",
        EventName = nameof(OperationWarning))]
    public static partial void OperationWarning(
        this ILogger logger,
        string operation,
        string containerName,
        int statusCode,
        string? activityId,
        double requestCharge,
        Exception exception);

    [LoggerMessage(
        EventId = 7204,
        Level = LogLevel.Error,
        Message = "Cosmos operation failed. Operation: {Operation}, Container: {ContainerName}, StatusCode: {StatusCode}, ActivityId: {ActivityId}, RequestCharge: {RequestCharge}",
        EventName = nameof(OperationError))]
    public static partial void OperationError(
        this ILogger logger,
        string operation,
        string containerName,
        int statusCode,
        string? activityId,
        double requestCharge,
        Exception exception);

    [LoggerMessage(
        EventId = 7205,
        Level = LogLevel.Warning,
        Message = "Cosmos operation was cancelled. Operation: {Operation}, Container: {ContainerName}",
        EventName = nameof(OperationCancelled))]
    public static partial void OperationCancelled(
        this ILogger logger,
        string operation,
        string containerName,
        Exception exception);

    [LoggerMessage(
        EventId = 7206,
        Level = LogLevel.Error,
        Message = "Cosmos operation failed unexpectedly. Operation: {Operation}, Container: {ContainerName}",
        EventName = nameof(UnexpectedOperationFailure))]
    public static partial void UnexpectedOperationFailure(
        this ILogger logger,
        string operation,
        string containerName,
        Exception exception);
}
