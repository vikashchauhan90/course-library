using System.Diagnostics.Metrics;

namespace CourseLibrary.App.Configuration.Observability.Metrics;

public static class Meters
{
    public const string Name =
       "CourseLibrary.App";

    public static readonly Meter App =
        new(Name);

    public static readonly Histogram<double> RequestDuration =
       App.CreateHistogram<double>(
           "course_library.app.request.duration",
           unit: "ms",
           description: "Duration of HTTP requests.");

    public static readonly Counter<long> RequestCount =
    App.CreateCounter<long>(
        "course_library.app.request.count",
        description: "Number of HTTP requests processed.");

    public static readonly Histogram<long> RequestBodySize =
    App.CreateHistogram<long>(
        "course_library.request.body.size",
        unit: "By",
        description: "Size of HTTP request bodies.");

    public static readonly Histogram<long> ResponseBodySize =
    App.CreateHistogram<long>(
        "course_library.response.body.size",
        unit: "By",
        description: "Size of HTTP responses.");

    public static readonly UpDownCounter<long> ActiveRequests =
    App.CreateUpDownCounter<long>(
        "course_library.request.active",
        description: "Number of HTTP requests currently being processed.");
}
