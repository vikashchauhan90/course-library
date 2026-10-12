
using Azure.Messaging.ServiceBus;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace CourseLibrary.Platform.Messaging.Observability;

public static class ServiceBusTraceContext
{
    public const string DiagnosticId = "Diagnostic-Id";
    public const string TraceParent = "traceparent";
    public const string TraceState = "tracestate";
    public const string EventId = "Id";
    public const string EventType = "Type";
    public const string EventOccurredAt = "OccurredAt";
    public const string Destination = "Destination";
    public const string UserId = "UserId";
    public const string Source = "source";
    public const string TenantId = "Tenant-Id";

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
                TraceParent,
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
}
