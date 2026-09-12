using Azure.Messaging.ServiceBus;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Observability.Traces;

public static class ServiceBusTraceContext
{
    public const string DiagnosticId = "Diagnostic-Id";
    public const string TraceParentHeader = "traceparent";
    public const string TraceStateHeader = "tracestate";
    public const string EventId = "Id";
    public const string EventType = "Type";
    public const string EventOccurredAt = "OccurredAt";
    public const string Destination = "Destination";
    public const string UserId = "UserId";
    public const string Source = "source";
    public static readonly TextMapPropagator Propagator =
        Propagators.DefaultTextMapPropagator;

    public static void Inject(
        ServiceBusMessage message,
        Activity? activity = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        activity ??= Activity.Current;

        if (activity is null)
        {
            return;
        }

        var carrier = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        Propagator.Inject(
            new PropagationContext(
                activity.Context,
                Baggage.Current),
            carrier,
            static (properties, key, value) =>
                properties[key] = value);

        foreach (var kvp in carrier)
        {
            message.ApplicationProperties[kvp.Key] = kvp.Value;
        }

        if (carrier.TryGetValue(
                TraceParentHeader,
                out var traceParent))
        {
            // Azure Service Bus correlation.
            message.ApplicationProperties[DiagnosticId] = traceParent;
        }
    }

    public static PropagationContext Extract(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var carrier = CreateCarrier(message);

        return Propagator.Extract(
            default,
            carrier,
            static (properties, key) =>
                properties.TryGetValue(key, out var value)
                    ? [value]
                    : []);
    }

    public static string? GetTraceParent(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return TryGetApplicationProperty(
            message,
            TraceParentHeader);
    }

    public static string? GetTraceState(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return TryGetApplicationProperty(
            message,
            TraceStateHeader);
    }

    public static string? GetDiagnosticId(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return TryGetApplicationProperty(
            message,
            DiagnosticId);
    }

    public static string? GetUserId(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return TryGetApplicationProperty(
            message,
            UserId);
    }

    public static string? getSource(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return TryGetApplicationProperty(
            message,
            Source);
    }
    private static Dictionary<string, string> CreateCarrier(
        ServiceBusReceivedMessage message)
    {
        var carrier = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var property in message.ApplicationProperties)
        {
            if (property.Value is string value)
            {
                carrier[property.Key] = value;
            }
        }

        return carrier;
    }

    private static string? TryGetApplicationProperty(
        ServiceBusReceivedMessage message,
        string propertyName)
    {
        foreach (var property in message.ApplicationProperties)
        {
            if (string.Equals(
                    property.Key,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase)
                && property.Value is string value)
            {
                return value;
            }
        }

        return null;
    }
}