using System.Diagnostics.Metrics;

namespace CourseLibrary.Infrastructure.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.Infrastructure";

    public static readonly Meter Infrastructure =
        new(Name);
}
