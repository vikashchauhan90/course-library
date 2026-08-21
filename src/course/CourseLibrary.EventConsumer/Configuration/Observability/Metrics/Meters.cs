using System.Diagnostics.Metrics;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.EventConsumer";

    public static readonly Meter EventConsumer =
        new(Name);
}
