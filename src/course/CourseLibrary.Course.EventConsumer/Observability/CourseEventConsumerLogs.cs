using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.EventConsumer.Observability;

internal static partial class CourseEventConsumerLogs
{
    [LoggerMessage(
        EventId = 7401,
        Level = LogLevel.Error,
        Message = "CourseEvent orchestration {InstanceId} received no event.",
        EventName = nameof(OrchestrationEventMissing))]
    public static partial void OrchestrationEventMissing(
        this ILogger logger,
        string instanceId);

    [LoggerMessage(
        EventId = 7402,
        Level = LogLevel.Warning,
        Message = "Duplicate CourseEvent message {MessageId}; orchestration {InstanceId} is {Status}.",
        EventName = nameof(DuplicateMessage))]
    public static partial void DuplicateMessage(
        this ILogger logger,
        string messageId,
        string instanceId,
        string status);

    [LoggerMessage(
        EventId = 7403,
        Level = LogLevel.Information,
        Message = "Purging stale CourseEvent orchestration {InstanceId} with status {Status}.",
        EventName = nameof(StaleOrchestrationPurgeStarted))]
    public static partial void StaleOrchestrationPurgeStarted(
        this ILogger logger,
        string instanceId,
        string? status);

    [LoggerMessage(
        EventId = 7404,
        Level = LogLevel.Warning,
        Message = "CourseEvent message {MessageId} lost the orchestration creation race for {InstanceId}.",
        EventName = nameof(OrchestrationCreationRaceLost))]
    public static partial void OrchestrationCreationRaceLost(
        this ILogger logger,
        string messageId,
        string instanceId);

    [LoggerMessage(
        EventId = 7405,
        Level = LogLevel.Information,
        Message = "Started CourseEvent orchestration {InstanceId} for message {MessageId}.",
        EventName = nameof(OrchestrationStarted))]
    public static partial void OrchestrationStarted(
        this ILogger logger,
        string instanceId,
        string messageId);

    [LoggerMessage(
        EventId = 7406,
        Level = LogLevel.Error,
        Message = "Failed to process CourseEvent message {MessageId}.",
        EventName = nameof(MessageProcessingFailed))]
    public static partial void MessageProcessingFailed(
        this ILogger logger,
        Exception exception,
        string messageId);

    [LoggerMessage(
        EventId = 7407,
        Level = LogLevel.Warning,
        Message = "Unable to deserialize dead-lettered CourseEvent {MessageId}.",
        EventName = nameof(DeadLetterDeserializationFailed))]
    public static partial void DeadLetterDeserializationFailed(
        this ILogger logger,
        Exception exception,
        string messageId);

    [LoggerMessage(
        EventId = 7408,
        Level = LogLevel.Error,
        Message = "Failed to process DQL message {MessageId}.",
        EventName = nameof(DeadLetterProcessingFailed))]
    public static partial void DeadLetterProcessingFailed(
        this ILogger logger,
        Exception exception,
        string messageId);

    [LoggerMessage(
        EventId = 7409,
        Level = LogLevel.Warning,
        Message = "Unable to create tracing activity for CourseEvent {EventId}.",
        EventName = nameof(ActivityCreationFailed))]
    public static partial void ActivityCreationFailed(
        this ILogger logger,
        string eventId);

    [LoggerMessage(
        EventId = 7410,
        Level = LogLevel.Error,
        Message = "Error processing CourseEvent {EventId} in orchestration {InstanceId}.",
        EventName = nameof(CourseEventProcessingFailed))]
    public static partial void CourseEventProcessingFailed(
        this ILogger logger,
        Exception exception,
        string eventId,
        string instanceId);

    [LoggerMessage(
        EventId = 7411,
        Level = LogLevel.Error,
        Message = "Failed to create course audit for CourseId {CourseId} and EventId {EventId}.",
        EventName = nameof(CourseAuditCreationFailed))]
    public static partial void CourseAuditCreationFailed(
        this ILogger logger,
        Exception exception,
        string courseId,
        string eventId);

    [LoggerMessage(
        EventId = 7412,
        Level = LogLevel.Debug,
        Message = "Started executing function {FunctionName}.",
        EventName = nameof(FunctionExecutionStarted))]
    public static partial void FunctionExecutionStarted(
        this ILogger logger,
        string functionName);

    [LoggerMessage(
        EventId = 7413,
        Level = LogLevel.Debug,
        Message = "Finished executing function {FunctionName} with outcome {Outcome} in {DurationMilliseconds} ms.",
        EventName = nameof(FunctionExecutionCompleted))]
    public static partial void FunctionExecutionCompleted(
        this ILogger logger,
        string functionName,
        string outcome,
        double durationMilliseconds);
}
