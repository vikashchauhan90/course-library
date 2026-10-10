using System.Diagnostics;

namespace CourseLibrary.Platform.Observability.Traces;

public abstract class ActivitySourceBase : IDisposable
{
    private readonly Lazy<ActivitySource> _activitySource;

    protected ActivitySourceBase(string sourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        _activitySource = new(
            () => new ActivitySource(sourceName),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    protected ActivitySource ActivitySource =>
        _activitySource.Value;

    public Activity? StartActivity(
        string activityName,
        ActivityKind kind = ActivityKind.Internal)
        => ActivitySource.StartActivity(
            activityName,
            kind);

    public void Dispose()
    {
        if (_activitySource.IsValueCreated)
        {
            _activitySource.Value.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}