using OpenTelemetry;
using System.Diagnostics;

namespace CourseLibrary.Platform.Observability.Traces.Processors;

public sealed class ActivityFailureProcessor
    : BaseProcessor<Activity>
{
    private const string ActivityFailedTag = "activity.failed";

    public override void OnEnd(Activity activity)
    {
        if (activity.Status != ActivityStatusCode.Error)
        {
            return;
        }

        activity.SetTag(ActivityFailedTag, true);
    }
}