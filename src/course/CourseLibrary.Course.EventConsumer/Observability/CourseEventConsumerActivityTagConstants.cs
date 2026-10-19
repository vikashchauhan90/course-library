namespace CourseLibrary.Course.EventConsumer.Observability;

internal static class CourseEventConsumerActivityTagConstants
{
    public const string MessagingSystem = "messaging.system";
    public const string MessagingDestination = "messaging.destination";
    public const string MessagingDestinationKind = "messaging.destination_kind";
    public const string MessagingOperation = "messaging.operation";
    public const string MessagingOperationType = "messaging.operation.type";
    public const string MessagingMessageId = "messaging.message_id";
    public const string MessagingCorrelationId = "messaging.correlation_id";
    public const string MessagingDeliveryCount = "messaging.delivery_count";
    public const string MessagingEventId = "messaging.event.id";
    public const string MessagingEventType = "messaging.event.type";
    public const string MessagingDuplicate = "messaging.duplicate";
    public const string MessagingDeadLetterReason = "messaging.dead_letter.reason";
    public const string MessagingCancelled = "messaging.cancelled";
    public const string OrchestrationName = "orchestration.name";
    public const string OrchestrationInstanceId = "orchestration.instance_id";
    public const string OrchestrationStartTime = "orchestration.start_time";
    public const string OrchestrationIsReplaying = "orchestration.is_replaying";
    public const string ActivityName = "activity.name";
    public const string ActivityEventType = "activity.event_type";
    public const string ActivityCourseId = "activity.course_id";
    public const string ActivityEventId = "activity.event_id";
}
