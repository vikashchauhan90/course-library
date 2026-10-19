using System.Diagnostics;

namespace CourseLibrary.Platform.Observability.Traces;

public static class ActivityContextHelper
{
    public static string? GetTraceId(
        Activity? activity = null)
    {
        activity ??= Activity.Current;

        return activity?.TraceId.ToString();
    }

    public static string? GetSpanId(
        Activity? activity = null)
    {
        activity ??= Activity.Current;

        return activity?.SpanId.ToString();
    }

    public static string? GetTraceParent(
        Activity? activity = null)
    {
        activity ??= Activity.Current;

        return activity?.Id;
    }

    public static string? GetTraceState(
        Activity? activity = null)
    {
        activity ??= Activity.Current;

        return activity?.TraceStateString;
    }

    public static ActivityContext GetContext(
        Activity? activity = null)
    {
        activity ??= Activity.Current;

        return activity?.Context ?? default;
    }
}