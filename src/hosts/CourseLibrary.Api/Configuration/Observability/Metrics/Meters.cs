using System.Diagnostics.Metrics;

namespace CourseLibrary.Api.Configuration.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.Api";

    public static readonly Meter Api =
        new(Name);

    public static readonly Histogram<double> RequestDuration =
       Api.CreateHistogram<double>(
           "course_library.api.request.duration",
           unit: "ms",
           description: "Duration of HTTP requests.");

    public static readonly Counter<long> RequestCount =
    Api.CreateCounter<long>(
        "course_library.api.request.count",
        description: "Number of HTTP requests processed.");

    public static readonly Histogram<long> RequestBodySize =
    Api.CreateHistogram<long>(
        "course_library.request.body.size",
        unit: "By",
        description: "Size of HTTP request bodies.");

    public static readonly Histogram<long> ResponseBodySize =
    Api.CreateHistogram<long>(
        "course_library.response.body.size",
        unit: "By",
        description: "Size of HTTP responses.");

    public static readonly UpDownCounter<long> ActiveRequests =
    Api.CreateUpDownCounter<long>(
        "course_library.request.active",
        description: "Number of HTTP requests currently being processed.");
}
