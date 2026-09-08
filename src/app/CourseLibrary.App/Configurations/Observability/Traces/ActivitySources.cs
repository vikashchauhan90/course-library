using System.Diagnostics;

namespace CourseLibrary.App.Configuration.Observability.Traces;

internal class ActivitySources
{
    public const string Name =
        "CourseLibrary.App";

    public static readonly ActivitySource App =
        new(Name);
}
