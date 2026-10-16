using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Messaging.Observability;

internal static partial class MessagingLogs
{
    [LoggerMessage(5501, LogLevel.Information, "Published event {EventType} with EventId {EventId} to {Destination} in {Duration}ms.", EventName = nameof(EventPublished))]
    public static partial void EventPublished(
        this ILogger logger,
        string eventType,
        Guid eventId,
        string destination,
        double duration);

    [LoggerMessage(5502, LogLevel.Debug, "Publishing event {EventId} was cancelled.", EventName = nameof(PublishCancelled))]
    public static partial void PublishCancelled(this ILogger logger, Guid eventId);

    [LoggerMessage(5503, LogLevel.Error, "Failed to publish event {EventType} with EventId {EventId}.", EventName = nameof(PublishFailed))]
    public static partial void PublishFailed(
        this ILogger logger,
        Exception exception,
        string eventType,
        Guid eventId);

    [LoggerMessage(5504, LogLevel.Warning, "Event {EventId} has invalid trace context metadata; ambient trace context will be used.", EventName = nameof(InvalidTraceContext))]
    public static partial void InvalidTraceContext(this ILogger logger, Guid eventId);

    [LoggerMessage(5505, LogLevel.Warning, "Message {EventId} size {Size} bytes exceeds the recommended Service Bus Standard tier limit.", EventName = nameof(MessageSizeExceedsRecommendedLimit))]
    public static partial void MessageSizeExceedsRecommendedLimit(
        this ILogger logger,
        Guid eventId,
        int size);

    [LoggerMessage(5506, LogLevel.Debug, "Dead-letter message {MessageId} stored with status {Status}.", EventName = nameof(DeadLetterStored))]
    public static partial void DeadLetterStored(
        this ILogger logger,
        string messageId,
        string status);

    [LoggerMessage(5507, LogLevel.Debug, "Storing dead-letter message {MessageId} was cancelled.", EventName = nameof(DeadLetterStoreCancelled))]
    public static partial void DeadLetterStoreCancelled(this ILogger logger, string messageId);

    [LoggerMessage(5508, LogLevel.Error, "Failed to store dead-letter message {MessageId}.", EventName = nameof(DeadLetterStoreFailed))]
    public static partial void DeadLetterStoreFailed(
        this ILogger logger,
        Exception exception,
        string messageId);
}
