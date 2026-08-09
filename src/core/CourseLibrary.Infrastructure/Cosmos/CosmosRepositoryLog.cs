using Microsoft.Extensions.Logging;

namespace CourseLibrary.Infrastructure.Cosmos;

internal static partial class CosmosRepositoryLog
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "Cosmos document was not found. Operation: {Operation}, Container: {ContainerName}, Id: {Id}")]
    public static partial void DocumentNotFound(
        this ILogger logger,
        string operation,
        string containerName,
        string id);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Cosmos operation failed. Operation: {Operation}, Container: {ContainerName}, StatusCode: {StatusCode}, ActivityId: {ActivityId}, RequestCharge: {RequestCharge}")]
    public static partial void OperationWarning(
        this ILogger logger,
        string operation,
        string containerName,
        int statusCode,
        string? activityId,
        double requestCharge,
        Exception exception);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Cosmos operation failed. Operation: {Operation}, Container: {ContainerName}, StatusCode: {StatusCode}, ActivityId: {ActivityId}, RequestCharge: {RequestCharge}")]
    public static partial void OperationError(
        this ILogger logger,
        string operation,
        string containerName,
        int statusCode,
        string? activityId,
        double requestCharge,
        Exception exception);
}