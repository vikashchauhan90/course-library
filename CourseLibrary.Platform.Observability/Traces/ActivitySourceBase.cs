using System.Diagnostics;

namespace CourseLibrary.Platform.Observability.Traces;

public abstract class ActivitySourceBase
{
    private readonly ActivitySource _activitySource;

    protected ActivitySourceBase(string sourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        _activitySource = new ActivitySource(sourceName);
    }

    protected ActivitySource ActivitySource =>
        _activitySource;

    public Activity? StartActivity(
        string activityName,
        ActivityKind kind = ActivityKind.Internal)
        => ActivitySource.StartActivity(
            activityName,
            kind);
}