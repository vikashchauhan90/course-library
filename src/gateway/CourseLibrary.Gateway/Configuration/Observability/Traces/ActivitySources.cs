using System.Diagnostics;

namespace CourseLibrary.Gateway.Configuration.Observability.Traces;

internal class ActivitySources
{
    public const string Name =
        "CourseLibrary.Gateway";

    public static readonly ActivitySource Gateway =
        new(Name);
}
