using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Course.Client.Observability;

public sealed class CourseClientMeter : MeterBase
{
    private static readonly Lazy<CourseClientMeter> SharedInstance = new(
        static () => new CourseClientMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CourseClientMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _requests;
    private readonly Counter<long> _failures;
    private readonly Histogram<long> _duration;
    public CourseClientMeter() : base(CourseClientMeterConstants.MeterName)
    {
        _requests= CreateCounter<long>(
            CourseClientMeterConstants.RequestsInstrumentName,
            "{request}",
            "Number of requests made to the Course API.");
        _failures = CreateCounter<long>(
            CourseClientMeterConstants.FailuresInstrumentName,
            "{failure}",
            "Number of failed requests made to the Course API.");
        _duration = CreateHistogram<long>(
            CourseClientMeterConstants.DurationInstrumentName,
            "{duration}",
            "Duration of requests made to the Course API.");
    }

    public void RecordRequest(string request)
    {
        _requests.Add(1, new("request", request));
    }
    public void RecordFailure(string failure)
    {
        _failures.Add(1, new("failure", failure));
    }
    public void RecordDuration(long duration)
    {
        _duration.Record(duration, new("duration", duration.ToString()));
    }

    private static TagList CreateTag(string key, string operation)
    {
        var tags = new TagList
        {
            { CourseClientActivityTagConstants., provider },
            { CourseClientActivityTagConstants.OperationTag, operation }
        };

        return tags;
    }

}
