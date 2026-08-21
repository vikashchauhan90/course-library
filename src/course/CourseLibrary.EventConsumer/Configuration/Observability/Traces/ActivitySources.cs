using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Traces;

internal class ActivitySources
{
    public const string Name =
        "CourseLibrary.EventConsumer";

    public static readonly ActivitySource EventConsumer =
        new(Name);
}
