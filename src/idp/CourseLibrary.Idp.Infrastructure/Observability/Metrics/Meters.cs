using System.Diagnostics.Metrics;

namespace CourseLibrary.Idp.Infrastructure.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.Idp.Infrastructure";

    public static readonly Meter Infrastructure =
        new(Name);
}
