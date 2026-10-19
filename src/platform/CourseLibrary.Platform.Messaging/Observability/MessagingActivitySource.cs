using System.Diagnostics;
using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.Messaging.Observability;

internal sealed class MessagingActivitySource : ActivitySourceBase
{
    private static readonly Lazy<MessagingActivitySource> SharedInstance = new(
        static () => new MessagingActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static MessagingActivitySource Shared => SharedInstance.Value;

    private MessagingActivitySource()
        : base(MessagingTelemetry.ActivitySourceName)
    {
    }

    public Activity? StartActivity(
        string activityName,
        ActivityKind kind,
        ActivityContext parentContext)
        => ActivitySource.StartActivity(activityName, kind, parentContext);
}
