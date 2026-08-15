using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Observability.Traces;

public static class ActivitySources
{
    public const string Name =
       "CourseLibrary.Infrastructure";

    public static readonly ActivitySource Infrastructure =
        new(Name);
}
