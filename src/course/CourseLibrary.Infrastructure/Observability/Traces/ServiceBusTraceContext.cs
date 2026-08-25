using Azure.Messaging.ServiceBus;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Observability.Traces;

public static class ServiceBusTraceContext
{
    private const string DiagnosticId = "Diagnostic-Id";
    private const string TraceParentHeader = "traceparent";

    public static void Inject(
        ServiceBusMessage message,
        Activity activity
        )
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        // If no activity is provided, try to use current activity
        activity ??= Activity.Current;
        if (activity == null)
        {
            return; // Nothing to inject
        }

        var carrier = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Propagators.DefaultTextMapPropagator.Inject(
            new PropagationContext(activity.Context, Baggage.Current),
            carrier,
            static (properties, key, value) => properties[key] = value
        );

        foreach (var kvp in carrier)
        {
            message.ApplicationProperties[kvp.Key] = kvp.Value;
        }

        // Add diagnostic id for Azure Service Bus correlation
        // Check for traceparent case-insensitively
        var traceParentKey = carrier.Keys.FirstOrDefault(
            k => string.Equals(k, TraceParentHeader, StringComparison.OrdinalIgnoreCase));

        if (traceParentKey != null && carrier.TryGetValue(traceParentKey, out var traceParent))
        {
            message.ApplicationProperties[DiagnosticId] = traceParent;
        }
    }

    public static PropagationContext Extract(ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        var carrier = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in message.ApplicationProperties)
        {
            if (kvp.Value is string value)
            {
                carrier[kvp.Key] = value;
            }
        }
        return Propagators.DefaultTextMapPropagator.Extract(
            default,
            carrier,
            static (properties, key) =>
            {
                // Case-insensitive lookup
                var actualKey = properties.Keys.FirstOrDefault(
                    k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

                if (actualKey != null && properties.TryGetValue(actualKey, out var value))
                {
                    return new[] { value };
                }

                return Array.Empty<string>();
            });
    }
}
