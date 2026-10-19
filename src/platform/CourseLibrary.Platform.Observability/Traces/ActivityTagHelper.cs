using System.Diagnostics;
namespace CourseLibrary.Platform.Observability.Traces;


public static class ActivityTagHelper
{
    public static void SetCorrelationId(
        Activity? activity,
        string correlationId)
        => activity?.SetTag(
            "correlation.id",
            correlationId);

    public static void SetRequestId(
        Activity? activity,
        string requestId)
        => activity?.SetTag(
            "request.id",
            requestId);
}