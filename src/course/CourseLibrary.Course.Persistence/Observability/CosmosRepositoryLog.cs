using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Persistence.Data.Extensions;

internal static partial class CosmosRepositoryLog
{
    [LoggerMessage(
        EventId = 7101,
        Level = LogLevel.Debug,
        Message = "Cosmos document was not found. Operation: {Operation}, Container: {ContainerName}, Id: {Id}",
        EventName = nameof(DocumentNotFound))]
    public static partial void DocumentNotFound(
        this ILogger logger,
        string operation,
        string containerName,
        string id);

    [LoggerMessage(
        EventId = 7102,
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
        EventId = 7103,
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
}