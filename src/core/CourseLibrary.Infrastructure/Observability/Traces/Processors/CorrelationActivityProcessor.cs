using OpenTelemetry;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Observability.Traces.Processors;

public sealed class CorrelationActivityProcessor
    : BaseProcessor<Activity>
{
    public override void OnStart(Activity activity)
    {
        if (activity is null)
        {
            return;
        }
        activity.SetTag("trace.id", activity.TraceId.ToString());
        activity.SetTag("span.id", activity.SpanId.ToString());

        if (activity.ParentSpanId != default)
        {
            activity.SetTag("parent.span.id", activity.ParentSpanId.ToString());
        }
        activity.SetTag("trace.state", activity.TraceStateString);
        activity.SetTag("trace.flags", activity.ActivityTraceFlags.ToString());
        activity.SetTag("trace.start_time", activity.StartTimeUtc.ToString("O"));
        activity.SetTag("trace.duration", activity.Duration.ToString());
    }
}