using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Traces;

internal static class RequestTraceContextFactory
{
    public static RequestTraceContext FromActivity(Activity? activity)
    {
        if (activity is null)
        {
            return new RequestTraceContext();
        }

        return new RequestTraceContext
        {
            TraceId = activity.TraceId.ToString(),
            TraceParent = activity.Id ?? string.Empty,
            ParentSpanId = activity.ParentSpanId.ToString(),
            TraceState = activity.TraceStateString ?? string.Empty,

            ActivityKind = activity.Kind,

            ActivityId = activity.SpanId.ToString(),
            ActivityParentId = activity.ParentSpanId.ToString(),

            ActivityState = activity.Status.ToString(),
            ActivityType = activity.OperationName,
            ActivityTypeName = activity.DisplayName
        };
    }

    public static ActivityContext ToActivityContext(
        RequestTraceContext? context)
    {
        if (context is null ||
            string.IsNullOrWhiteSpace(context.TraceParent))
        {
            return Activity.Current?.Context ?? default;
        }

        return ActivityContext.TryParse(
            context.TraceParent,
            context.TraceState,
            out var activityContext)
                ? activityContext
                : default;
    }
}