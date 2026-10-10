using System.Diagnostics;

namespace CourseLibrary.Platform.Observability.Traces;

public static class ActivityEnrichmentHelper
{
    public static void MarkSuccess(
        Activity? activity)
    {
        activity?.SetStatus(
            ActivityStatusCode.Ok);
    }

    public static void MarkFailure(
        Activity? activity,
        string description)
    {
        activity?.SetStatus(
            ActivityStatusCode.Error,
            description);
    }

    public static void RecordException(
        Activity? activity,
        Exception exception,
        TagList tags = default,
        DateTimeOffset timestamp = default)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetStatus(
            ActivityStatusCode.Error,
            exception.Message);

        activity.AddException(
            exception,
            tags,
            timestamp);
    }
}