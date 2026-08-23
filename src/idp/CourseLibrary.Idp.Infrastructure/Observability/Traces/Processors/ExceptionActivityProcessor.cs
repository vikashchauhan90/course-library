using OpenTelemetry;
using System.Diagnostics;

namespace CourseLibrary.Idp.Infrastructure.Observability.Traces.Processors;

public sealed class ExceptionActivityProcessor
    : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        if (activity is null)
        {
            return;
        }

        if (activity.Status == ActivityStatusCode.Error)
        {
            activity.SetTag("activity.failed", true);
        }
    }
}