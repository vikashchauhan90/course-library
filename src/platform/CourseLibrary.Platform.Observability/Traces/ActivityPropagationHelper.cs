using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;


namespace CourseLibrary.Platform.Observability.Traces;

public static class ActivityPropagationHelper
{
    public static void Inject(
       ActivityContext context,
       IDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        Propagators.DefaultTextMapPropagator.Inject(
            new PropagationContext(context, Baggage.Current),
            headers,
            static (carrier, key, value) => carrier[key] = value);
    }

    public static void InjectCurrent(IDictionary<string, string> headers)
    {
        var current = Activity.Current;
        if (current is null)
        {
            return;
        }

        Inject(current.Context, headers);
    }

    public static PropagationContext Extract(IDictionary<string, string> headers)
       => Extract(default, headers);

    public static PropagationContext Extract(
       ActivityContext parent,
       IDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        return Propagators.DefaultTextMapPropagator.Extract(
            new PropagationContext(parent, Baggage.Current),
            headers,
            static (carrier, key) =>
                carrier.TryGetValue(key, out var value) ? [value] : null);
    }
}