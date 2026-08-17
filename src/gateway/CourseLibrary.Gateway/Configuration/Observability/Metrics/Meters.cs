using System.Diagnostics.Metrics;

namespace CourseLibrary.Gateway.Configuration.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.Gateway";

    public static readonly Meter Gateway =
        new(Name);
}
