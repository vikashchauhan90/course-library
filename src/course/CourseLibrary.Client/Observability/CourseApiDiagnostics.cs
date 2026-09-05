using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Client.Observability;

public static class CourseApiDiagnostics
{
    public const string ActivitySourceName = "CourseLibrary.Client.CourseApi";
    public const string MeterName = "CourseLibrary.Client.CourseApi";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly Meter Meter = new(MeterName);
    public static readonly Counter<long> Requests = Meter.CreateCounter<long>("course_api.client.requests");
    public static readonly Counter<long> Failures = Meter.CreateCounter<long>("course_api.client.failures");
    public static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("course_api.client.duration", "ms");
}
