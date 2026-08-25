using System.Diagnostics.Metrics;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.EventConsumer";

    public static readonly Meter EventConsumer =
        new(Name);

    public static readonly Counter<long> FunctionCount =
        EventConsumer.CreateCounter<long>(
            "function.execution.count");

    public static readonly Histogram<double> FunctionDuration =
        EventConsumer.CreateHistogram<double>(
            "function.execution.duration",
            "ms");

    public static readonly UpDownCounter<long> ActiveFunctions =
        EventConsumer.CreateUpDownCounter<long>(
            "function.execution.active");

    // Service Bus Consumer Metrics
    public static readonly Counter<int> ServiceBusEventsConsumed =
        EventConsumer.CreateCounter<int>(
            "servicebus.events.consumed",
            "events",
            "Number of events consumed from Service Bus");

    public static readonly Counter<int> ServiceBusConsumeFailed =
        EventConsumer.CreateCounter<int>(
            "servicebus.events.consume_failed",
            "events",
            "Number of failed event consumption attempts");

    public static readonly Counter<int> ServiceBusMessagesCompleted =
        EventConsumer.CreateCounter<int>(
            "servicebus.messages.completed",
            "messages",
            "Number of messages successfully completed");

    public static readonly Counter<int> ServiceBusMessagesDeadLettered =
        EventConsumer.CreateCounter<int>(
            "servicebus.messages.dead_lettered",
            "messages",
            "Number of messages sent to dead letter queue");

    public static readonly Counter<int> ServiceBusMessagesAbandoned =
        EventConsumer.CreateCounter<int>(
            "servicebus.messages.abandoned",
            "messages",
            "Number of messages abandoned for retry");

    public static readonly Histogram<double> ServiceBusConsumeDuration =
       EventConsumer.CreateHistogram<double>(
           "servicebus.consume.duration",
           "ms",
           "Duration of Service Bus message processing in milliseconds");

    public static readonly Histogram<double> ServiceBusMessageProcessingDuration =
        EventConsumer.CreateHistogram<double>(
            "servicebus.message.processing_duration",
            "ms",
            "Total time to process a Service Bus message");

    public static readonly Counter<int> ServiceBusMessagesProcessed =
        EventConsumer.CreateCounter<int>(
            "servicebus.messages.processed",
            "messages",
            "Total number of Service Bus messages processed");

    // Durable Functions Metrics
    public static readonly Counter<int> OrchestrationsScheduled =
        EventConsumer.CreateCounter<int>(
            "durable.orchestrations.scheduled",
            "orchestrations",
            "Number of orchestrations scheduled");

    public static readonly Counter<int> OrchestrationsCompleted =
        EventConsumer.CreateCounter<int>(
            "durable.orchestrations.completed",
            "orchestrations",
            "Number of orchestrations completed successfully");

    public static readonly Counter<int> OrchestrationsFailed =
        EventConsumer.CreateCounter<int>(
            "durable.orchestrations.failed",
            "orchestrations",
            "Number of orchestrations that failed");

    public static readonly Histogram<double> OrchestrationDuration =
        EventConsumer.CreateHistogram<double>(
            "durable.orchestration.duration",
            "ms",
            "Duration of orchestration execution in milliseconds");

    public static readonly Counter<int> ActivitiesScheduled =
        EventConsumer.CreateCounter<int>(
            "durable.activities.scheduled",
            "activities",
            "Number of activities scheduled");

    public static readonly Counter<int> ActivitiesCompleted =
        EventConsumer.CreateCounter<int>(
            "durable.activities.completed",
            "activities",
            "Number of activities completed successfully");

    public static readonly Counter<int> ActivitiesFailed =
        EventConsumer.CreateCounter<int>(
            "durable.activities.failed",
            "activities",
            "Number of activities that failed");

    public static readonly Histogram<double> ActivityDuration =
        EventConsumer.CreateHistogram<double>(
            "durable.activity.duration",
            "ms",
            "Duration of activity execution in milliseconds");

    // Common/Utility Metrics
    public static readonly Counter<int> DeserializationFailures =
        EventConsumer.CreateCounter<int>(
            "messaging.deserialization_failures",
            "errors",
            "Number of message deserialization failures");

    public static readonly Counter<int> DuplicateMessagesDetected =
        EventConsumer.CreateCounter<int>(
            "messaging.duplicate_messages",
            "messages",
            "Number of duplicate messages detected");

    public static readonly Histogram<double> SerializationDuration =
        EventConsumer.CreateHistogram<double>(
            "messaging.serialization.duration",
            "ms",
            "Duration of message serialization operations");

    public static readonly Histogram<double> DeserializationDuration =
        EventConsumer.CreateHistogram<double>(
            "messaging.deserialization.duration",
            "ms",
            "Duration of message deserialization operations");

    public static KeyValuePair<string, object?>[] CreateOrchestrationTags(
        string orchestrationName,
        string instanceId,
        string? status = null)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("orchestration_name", orchestrationName),
            new("instance_id", instanceId)
        };

        if (!string.IsNullOrEmpty(status))
        {
            tags.Add(new KeyValuePair<string, object?>("status", status));
        }

        return tags.ToArray();
    }

    public static void RecordMessageConsumed(
        string eventType,
        string eventId,
        string destination)
    {
        var tags = CreateEventTags(eventType, eventId, destination);

        ServiceBusEventsConsumed.Add(1, tags);
    }

    public static void RecordMessageProcessed(
        string eventType,
        string eventId,
        string destination,
        double durationMs)
    {
        var tags = CreateEventTags(eventType, eventId, destination);

        ServiceBusMessagesProcessed.Add(1, tags);
        ServiceBusMessageProcessingDuration.Record(durationMs, tags);
    }

    public static void RecordMessageFailed(
        string eventType,
        string eventId,
        string destination,
        Exception exception)
    {
        var tags = CreateEventTags(eventType, eventId, destination)
            .Concat(CreateErrorTags(exception))
            .ToArray();

        ServiceBusConsumeFailed.Add(1, tags);
    }

    public static void RecordOrchestrationStarted(
       string orchestrationName,
       string instanceId)
    {
        OrchestrationsScheduled.Add(1,
            CreateOrchestrationTags(orchestrationName, instanceId, "started"));
    }

    public static void RecordOrchestrationCompleted(
        string orchestrationName,
        string instanceId,
        double durationMs)
    {
        var tags = CreateOrchestrationTags(orchestrationName, instanceId, "completed");

        OrchestrationsCompleted.Add(1, tags);
        OrchestrationDuration.Record(durationMs, tags);
    }

    public static void RecordOrchestrationFailed(
        string orchestrationName,
        string instanceId,
        Exception exception)
    {
        var tags = CreateOrchestrationTags(orchestrationName, instanceId, "failed")
            .Concat(CreateErrorTags(exception))
            .ToArray();

        OrchestrationsFailed.Add(1, tags);
    }

    public static KeyValuePair<string, object?>[] CreateEventTags(
        string eventType,
        string eventId,
        string? destination = null)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("event_type", eventType),
            new("event_id", eventId)
        };

        if (!string.IsNullOrEmpty(destination))
        {
            tags.Add(new KeyValuePair<string, object?>("destination", destination));
        }

        return tags.ToArray();
    }

    public static KeyValuePair<string, object?>[] CreateErrorTags(
        Exception exception)
    {
        return new[]
        {
            new KeyValuePair<string, object?>("error_type", exception.GetType().Name),
            new KeyValuePair<string, object?>("error_message", exception.Message)
        };
    }
}
