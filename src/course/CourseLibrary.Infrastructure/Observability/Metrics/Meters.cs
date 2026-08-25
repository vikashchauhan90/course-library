using System.Diagnostics.Metrics;

namespace CourseLibrary.Infrastructure.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.Infrastructure";

    public static readonly Meter Infrastructure =
        new(Name);

    // Service Bus Publisher Metrics
    public static readonly Counter<int> ServiceBusEventsPublished =
        Infrastructure.CreateCounter<int>(
            "servicebus.events.published",
            "events",
            "Number of events published to Service Bus");

    public static readonly Counter<int> ServiceBusPublishFailed =
        Infrastructure.CreateCounter<int>(
            "servicebus.events.publish_failed",
            "events",
            "Number of failed event publish attempts");

    public static readonly Histogram<double> ServiceBusPublishDuration =
        Infrastructure.CreateHistogram<double>(
            "servicebus.publish.duration",
            "ms",
            "Duration of Service Bus publish operations in milliseconds");

    public static readonly Histogram<int> ServiceBusMessageSize =
        Infrastructure.CreateHistogram<int>(
            "servicebus.message.size",
            "bytes",
            "Size of published Service Bus messages in bytes");

    public static readonly Counter<long> ServiceBusBytesPublished =
        Infrastructure.CreateCounter<long>(
            "servicebus.events.bytes_published",
            "bytes",
            "Total bytes published to Service Bus");


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

    public static KeyValuePair<string, object?>[] CreateMessageTags(
       string messageId,
       string? queueName = null,
       string? subscriptionName = null)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("message_id", messageId)
        };

        if (!string.IsNullOrEmpty(queueName))
        {
            tags.Add(new KeyValuePair<string, object?>("queue", queueName));
        }

        if (!string.IsNullOrEmpty(subscriptionName))
        {
            tags.Add(new KeyValuePair<string, object?>("subscription", subscriptionName));
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
