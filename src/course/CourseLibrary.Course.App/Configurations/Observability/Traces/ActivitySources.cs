using System.Diagnostics;

namespace CourseLibrary.Course.App.Configuration.Observability.Traces;

internal class ActivitySources
{
    public const string Name =
        "CourseLibrary.Course.App";

    public static readonly ActivitySource App =
        new(Name);
}
