using System.Diagnostics;

namespace CourseLibrary.Platform.Observability.Traces;

public abstract class ActivitySourceBase : IDisposable
{
    private readonly ActivitySource _activitySource;
    private int _disposed;

    protected ActivitySourceBase(string sourceName, string? version = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        if (Activity.DefaultIdFormat != ActivityIdFormat.W3C)
        {
            throw new InvalidOperationException(
                $"{nameof(ActivitySourceBase)} requires W3C activity IDs. " +
                $"Call {nameof(ActivityDefaults)}.{nameof(ActivityDefaults.EnsureW3C)}() during application startup. " +
                $"Current format: {Activity.DefaultIdFormat}.");
        }

        _activitySource = new ActivitySource(sourceName, version ?? string.Empty);
    }

    protected ActivitySource ActivitySource => _activitySource;

    public string Name => _activitySource.Name;

    public bool HasListeners()
        => _activitySource.HasListeners();

    public Activity? StartActivity(
        string activityName,
        ActivityKind kind = ActivityKind.Internal)
        => StartActivityCore(
            activityName,
            kind,
            parentContext: default);

    public Activity? StartActivity(
        string activityName,
        ActivityKind kind,
        ActivityContext parentContext)
        => StartActivityCore(
            activityName,
            kind,
            parentContext);

    public Activity? StartRemote(
        string activityName,
        ActivityKind kind,
        string traceParent,
        string? traceState = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traceParent);

        ActivityContext parentContext =
            ActivityContextHelper.GetContext(Activity.Current);

        if (ActivityContext.TryParse(
                traceParent,
                traceState,
                out ActivityContext remoteContext))
        {
            parentContext = remoteContext;
        }

        return StartActivityCore(
            activityName,
            kind,
            parentContext);
    }

    protected virtual void Enrich(Activity activity)
    {
        activity.SetTag("library.name", "CourseLibrary");
        activity.SetTag("library.layer", "Platform");
        activity.SetTag("activity.source", ActivitySource.Name);
        activity.SetTag("parent.traceId", activity.TraceId.ToString());
        activity.SetTag("parent.spanId", activity.SpanId.ToString());
        activity.SetTag("activity.kind", activity.Kind.ToString());
    }

    protected virtual void OnActivityStarted(Activity activity)
    {
        activity.SetTag("activity.started", DateTime.UtcNow.ToString("o"));
        activity.SetTag("activity.status", "started");
    }

    protected virtual void OnActivityStopped(Activity activity)
    {
        activity.SetTag("activity.stopped", DateTime.UtcNow.ToString("o"));
        activity.SetTag("activity.status", "stopped");
        activity.SetTag("activity.duration", activity.Duration.ToString());
    }

    private Activity? StartActivityCore(
        string activityName,
        ActivityKind kind,
        ActivityContext parentContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityName);

        Activity? activity =
            parentContext == default
                ? _activitySource.StartActivity(activityName, kind)
                : _activitySource.StartActivity(
                    activityName,
                    kind,
                    parentContext);

        if (activity is null)
        {
            return null;
        }

        Enrich(activity);
        OnActivityStarted(activity);

        return activity;
    }

    protected void StopActivity(Activity? activity)
    {
        if (activity is null)
        {
            return;
        }

        OnActivityStopped(activity);
        activity.Stop();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        if (disposing)
        {
            _activitySource.Dispose();
        }
    }
}