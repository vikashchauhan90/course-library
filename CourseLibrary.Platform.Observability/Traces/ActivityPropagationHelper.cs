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
        Propagators.DefaultTextMapPropagator.Inject(
            new PropagationContext(
                context,
                Baggage.Current),
            headers,
            static (carrier, key, value)
                => carrier[key] = value);
    }

    public static PropagationContext Extract(
        IDictionary<string, string> headers)
    {
        return Propagators.DefaultTextMapPropagator.Extract(
            default,
            headers,
            static (carrier, key) =>
            {
                return carrier.TryGetValue(key, out var value)
                    ? [value]
                    : [];
            });
    }
}