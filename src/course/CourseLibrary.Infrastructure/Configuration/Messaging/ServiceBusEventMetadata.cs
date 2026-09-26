using Azure.Messaging.ServiceBus;
using CourseLibrary.Infrastructure.Observability.Traces;

namespace CourseLibrary.Infrastructure.Configuration.Messaging;

public static class ServiceBusEventMetadata
{
    public static EventMetadata FromMessage(ServiceBusReceivedMessage message)
    {
        return new EventMetadata(
            EventId: GetProperty<string>(
                message,
                ServiceBusTraceContext.EventId) ?? string.Empty,

            EventOccurredAt: GetProperty<long>(
                message,
                ServiceBusTraceContext.EventOccurredAt),

            EventType: GetProperty<string>(
                message,
                ServiceBusTraceContext.EventType) ?? string.Empty,

            Destination: GetProperty<string>(
                message,
                ServiceBusTraceContext.Destination) ?? string.Empty,

            UserId: GetProperty<string>(
                message,
                ServiceBusTraceContext.UserId) ?? string.Empty,

            Source: GetProperty<string>(
                message,
                ServiceBusTraceContext.Source) ?? string.Empty,

            CorrelationId: message.CorrelationId,

            TraceParent: GetProperty<string>(
                message,
                ServiceBusTraceContext.TraceParent) ?? string.Empty,

            TraceState: GetProperty<string>(
                message,
                ServiceBusTraceContext.TraceState) ?? string.Empty);
    }

    private static T? GetProperty<T>(
        ServiceBusReceivedMessage message,
        string key)
    {
        message.ApplicationProperties.TryGetValue(
                key,
                out var value);

        return value switch
        {
            T typedValue => typedValue,

            _ => default(T),
        };

    }
}