using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Traces;

internal class ActivitySources
{
    public const string Name =
        "CourseLibrary.EventConsumer";

    public static readonly ActivitySource EventConsumer =
        new(Name);


    public static Activity? StartActivity(
    string name,
    ActivityKind kind,
    string? parentTraceParent,
    string? parentTraceState = null)
    {
        if (string.IsNullOrWhiteSpace(parentTraceParent))
        {
            return EventConsumer.StartActivity(name, kind);
        }

        if (!ActivityContext.TryParse(
                parentTraceParent,
                parentTraceState,
                isRemote: true,
                out var parentContext))
        {
            return EventConsumer.StartActivity(name, kind);
        }

        return EventConsumer.StartActivity(
            name,
            kind,
            parentContext);
    }

    public static Activity? StartActivity(
    string name,
    ActivityKind kind,
    string? parentActivityId)
    {
        if (string.IsNullOrEmpty(parentActivityId))
        {
            return EventConsumer.StartActivity(name, kind);
        }

        return EventConsumer.StartActivity(
            name,
            kind,
            parentActivityId);
    }

    public static Activity? StartActivity(
        string name,
        ActivityKind kind,
        ActivityContext? parentContext)
    {
        if (parentContext == null)
        {
            return EventConsumer.StartActivity(name, kind);
        }

        return EventConsumer.StartActivity(
            name,
            kind,
            parentContext.Value);
    }
}
