using System.Diagnostics;

namespace CourseLibrary.Platform.Observability.Traces;

public static class ActivityContextHelper
{
    public static string? GetTraceId(Activity? activity = null)
        => GetW3CActivity(activity)?.TraceId.ToString();

    public static string? GetSpanId(Activity? activity = null)
        => GetW3CActivity(activity)?.SpanId.ToString();

    public static string? GetTraceParent(Activity? activity = null)
        => GetW3CActivity(activity)?.Id;

    public static string? GetTraceState(Activity? activity = null)
        => GetW3CActivity(activity)?.TraceStateString;

    public static ActivityContext GetContext(Activity? activity = null)
        => GetW3CActivity(activity)?.Context ?? default;

    public static bool IsW3CFormat(this Activity? activity)
        => activity?.IdFormat == ActivityIdFormat.W3C;

    private static Activity? GetW3CActivity(Activity? activity)
    {
        activity ??= Activity.Current;
        return activity is not null && activity.IdFormat == ActivityIdFormat.W3C
            ? activity
            : null;
    }
}