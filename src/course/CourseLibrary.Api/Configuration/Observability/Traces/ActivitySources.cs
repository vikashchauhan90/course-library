using System.Diagnostics;

namespace CourseLibrary.Api.Configuration.Observability.Traces;

internal class ActivitySources
{
    public const string Name =
        "CourseLibrary.Api";

    public static readonly ActivitySource Api =
        new(Name);
}
