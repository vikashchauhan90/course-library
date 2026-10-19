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
    private readonly Histogram<long> _requestBodySize;
    private readonly Histogram<long> _responseBodySize;
    private readonly UpDownCounter<long> _activeRequests;

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
        _requestBodySize = CreateHistogram<long>(
            "course.api.request.body.size",
            "By",
            "Size of API request bodies.");
        _responseBodySize = CreateHistogram<long>(
            "course.api.response.body.size",
            "By",
            "Size of API response bodies.");
        _activeRequests = CreateUpDownCounter<long>(
            "course.api.requests.active",
            "{request}",
            "Number of API requests currently being handled.");
    }

    public void RequestStarted() => _activeRequests.Add(1);

    public void RequestCompleted() => _activeRequests.Add(-1);

    public void RecordRequest(
        string endpointName,
        string method,
        string outcome,
        double durationMilliseconds,
        long? requestBodySize,
        long? responseBodySize)
    {
        var tags = new TagList
        {
            { CourseApiActivityTagConstants.EndpointName, endpointName },
            { CourseApiActivityTagConstants.HttpRequestMethod, method },
            { CourseApiActivityTagConstants.EndpointOutcome, outcome }
        };

        _requests.Add(1, tags);
        _duration.Record(durationMilliseconds, tags);

        if (requestBodySize is long requestSize)
        {
            _requestBodySize.Record(requestSize, tags);
        }

        if (responseBodySize is long responseSize)
        {
            _responseBodySize.Record(responseSize, tags);
        }
    }
}
