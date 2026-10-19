using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Course.Api.Observability;

internal sealed class CourseApiMeter : MeterBase
{
    private static readonly Lazy<CourseApiMeter> SharedInstance = new(
        static () => new CourseApiMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly Counter<long> _requests;
    private readonly Histogram<double> _duration;

    public static CourseApiMeter Shared => SharedInstance.Value;

    private CourseApiMeter()
        : base(CourseApiTelemetry.MeterName)
    {
        _requests = CreateCounter<long>(
            "course.api.endpoint.requests",
            "{request}",
            "Number of API endpoint requests.");
        _duration = CreateHistogram<double>(
            "course.api.endpoint.duration",
            "ms",
            "Duration of API endpoint requests.");
    }

    public void RecordRequest(
        string endpointName,
        string method,
        string outcome,
        double durationMilliseconds)
    {
        var tags = new TagList
        {
            { CourseApiActivityTagConstants.EndpointName, endpointName },
            { CourseApiActivityTagConstants.HttpRequestMethod, method },
            { CourseApiActivityTagConstants.EndpointOutcome, outcome }
        };

        _requests.Add(1, tags);
        _duration.Record(durationMilliseconds, tags);
    }
}
