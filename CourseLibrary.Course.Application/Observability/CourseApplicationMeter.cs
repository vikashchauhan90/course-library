using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Course.Application.Observability;

internal sealed class CourseApplicationMeter : MeterBase
{
    private static readonly Lazy<CourseApplicationMeter> SharedInstance = new(
        static () => new CourseApplicationMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CourseApplicationMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _operations;
    private readonly Counter<long> _failures;
    private readonly Histogram<double> _duration;

    private CourseApplicationMeter()
        : base(CourseApplicationTelemetry.MeterName)
    {
        _operations = CreateCounter<long>(
            CourseApplicationMetricConstants.OperationsInstrumentName,
            "{operation}",
            "Number of application operations completed.");
        _failures = CreateCounter<long>(
            CourseApplicationMetricConstants.FailuresInstrumentName,
            "{failure}",
            "Number of failed or cancelled application operations.");
        _duration = CreateHistogram<double>(
            CourseApplicationMetricConstants.DurationInstrumentName,
            "ms",
            "Duration of application operations.");
    }

    public void RecordOperation(string operation, string outcome, double durationMilliseconds)
    {
        var tags = CreateTags(operation, outcome);
        _operations.Add(1, tags);
        _duration.Record(durationMilliseconds, tags);
    }

    public void RecordFailure(string operation, string outcome)
        => _failures.Add(1, CreateTags(operation, outcome));

    private static TagList CreateTags(string operation, string outcome)
    {
        var tags = new TagList
        {
            { CourseApplicationActivityTagConstants.Operation, operation },
            { "app.operation.outcome", outcome }
        };

        return tags;
    }
}
