using Azure.Messaging.ServiceBus;
using CourseLibrary.Infrastructure.Observability.Traces;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Traces;

internal static class ServiceBusTraceContext
{
    public static ActivityContext Extract(
        string? traceParent,
        string? traceState)
    {
        if (string.IsNullOrWhiteSpace(traceParent))
        {
            return default;
        }

        return ActivityContext.TryParse(
            traceParent,
            traceState,
            out var context)
                ? context
                : default;
    }

    public static ActivityContext Extract(
       ServiceBusReceivedMessage message)
    {
        var traceParent = GetProperty(
            message,
            TraceHeaders.TraceParent);

        var traceState = GetProperty(
            message,
            TraceHeaders.TraceState);

        return Extract(
            traceParent,
            traceState);
    }

    private static string? GetProperty(
       ServiceBusReceivedMessage message,
       string name)
    {
        return message.ApplicationProperties.TryGetValue(
            name,
            out var value)
                ? value?.ToString()
                : null;
    }
}