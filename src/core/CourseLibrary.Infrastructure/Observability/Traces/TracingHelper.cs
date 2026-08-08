using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CourseLibrary.Infrastructure.Observability.Traces;

public static class TracingHelper
{
    public static string GenerateTraceIdentifier(Activity? activity)
    {
        if (activity != null && activity.TraceId != default)
        {
            return activity.TraceId.ToString();
        }

        return ActivityTraceId.CreateRandom().ToString();
    }

    public static string GenerateTraceParent(Activity? activity)
    {
        if (activity != null && !string.IsNullOrEmpty(activity.Id))
        {
            return activity.Id;
        }

        var traceId = ActivityTraceId.CreateRandom();
        var spanId = ActivitySpanId.CreateRandom();

        return $"00-{traceId}-{spanId}-01";
    }
    public static string GenerateTraceState(
    Activity? activity,
    string defaultValue = "")
    {
        return activity?.TraceStateString ?? defaultValue;
    }
}
