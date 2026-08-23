using System.Diagnostics;

namespace CourseLibrary.Idp.Infrastructure.Observability.Traces;

public static class ActivitySources
{
    public const string Name =
       "CourseLibrary.Idp.Infrastructure";

    public static readonly ActivitySource Infrastructure =
        new(Name);
}
