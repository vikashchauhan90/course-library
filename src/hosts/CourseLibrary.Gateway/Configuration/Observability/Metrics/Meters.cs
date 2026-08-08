using System.Diagnostics.Metrics;

namespace CourseLibrary.Gateway.Configuration.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.Api";

    public static readonly Meter Api =
        new(Name);
}
