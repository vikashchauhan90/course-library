namespace CourseLibrary.Platform.Messaging.Observability;

internal static class MessagingActivityTagConstants
{
    public const string Operation = "messaging.operation";
    public const string Outcome = "messaging.outcome";
    public const string System = "messaging.system";
    public const string Destination = "messaging.destination.name";
    public const string MessageId = "messaging.message.id";
    public const string EventType = "event.type";
    public const string ContentType = "message.content_type";
    public const string MessageSize = "message.size";
    public const string DeliveryCount = "messaging.message.delivery_count";
    public const string Status = "messaging.message.status";
    public const string Cancelled = "messaging.cancelled";
    public const string ErrorType = "error.type";

    public const string PublishOperation = "publish";
    public const string StoreDeadLetterOperation = "store-dead-letter";
}
