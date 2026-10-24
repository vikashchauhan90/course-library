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
            CourseApplicationMetricConstants.UseCasesInstrumentName,
            "{useCase}",
            "Number of application use cases completed.");
        _failures = CreateCounter<long>(
            CourseApplicationMetricConstants.FailuresInstrumentName,
            "{failure}",
            "Number of failed or cancelled application use cases.");
        _duration = CreateHistogram<double>(
            CourseApplicationMetricConstants.DurationInstrumentName,
            "ms",
            "Duration of application use cases.");
    }

    public void RecordUseCase(string useCase, string outcome, double durationMilliseconds)
    {
        var tags = CreateTags(useCase, outcome);
        _operations.Add(1, tags);
        _duration.Record(durationMilliseconds, tags);
    }

    public void RecordFailure(string useCase, string outcome)
        => _failures.Add(1, CreateTags(useCase, outcome));

    private static TagList CreateTags(string useCase, string outcome)
    {
        var tags = new TagList
        {
            { CourseApplicationActivityTagConstants.UseCase, useCase },
            { "app.use_case.outcome", outcome }
        };

        return tags;
    }
}
