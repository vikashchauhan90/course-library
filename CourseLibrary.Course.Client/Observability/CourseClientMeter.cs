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
    private readonly Histogram<double> _duration;

    private CourseClientMeter() : base(CourseClientMeterConstants.MeterName)
    {
        _requests = CreateCounter<long>(
            CourseClientMeterConstants.RequestsInstrumentName,
            "{request}",
            "Number of requests made to the Course API.");
        _failures = CreateCounter<long>(
            CourseClientMeterConstants.FailuresInstrumentName,
            "{failure}",
            "Number of failed requests made to the Course API.");
        _duration = CreateHistogram<double>(
            CourseClientMeterConstants.DurationInstrumentName,
            "ms",
            "Duration of requests made to the Course API.");
    }

    public void RecordRequest(string operation)
    {
        _requests.Add(1, CreateTags(operation));
    }

    public void RecordFailure(string operation, string failureType)
    {
        _failures.Add(1, CreateTags(operation, failureType));
    }

    public void RecordDuration(double durationMilliseconds, string operation)
    {
        _duration.Record(durationMilliseconds, CreateTags(operation));
    }

    private static TagList CreateTags(string operation, string? failureType = null)
    {
        var tags = new TagList
        {
            { CourseClientActivityTagConstants.Operation, operation }
        };

        if (failureType is not null)
        {
            tags.Add(CourseClientActivityTagConstants.FailureType, failureType);
        }

        return tags;
    }
}
