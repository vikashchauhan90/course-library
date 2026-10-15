using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Course.Persistence.Observability;

internal sealed class CourseMeter : MeterBase
{
    private static readonly Lazy<CourseMeter> SharedInstance = new(
        static () => new CourseMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly Counter<long> _operations;
    private readonly Counter<long> _failures;
    private readonly Histogram<double> _duration;

    internal static CourseMeter Shared => SharedInstance.Value;

    private CourseMeter()
        : base(CoursePersistenceTelemetry.MeterName)
    {
        _operations = CreateCounter<long>(
            "course.persistence.repository.operations",
            "{operation}",
            "Number of course repository operations.");
        _failures = CreateCounter<long>(
            "course.persistence.repository.failures",
            "{failure}",
            "Number of failed or cancelled course repository operations.");
        _duration = CreateHistogram<double>(
            "course.persistence.repository.duration",
            "ms",
            "Duration of course repository operations.");
    }

    internal void RecordOperation(
        string repository,
        string operation,
        string outcome,
        double durationMilliseconds,
        string? failureType = null)
    {
        var tags = CreateTags(repository, operation, outcome);
        _operations.Add(1, tags);
        _duration.Record(durationMilliseconds, tags);

        if (failureType is not null)
        {
            _failures.Add(
                1,
                CreateTags(repository, operation, outcome, failureType));
        }
    }

    private static TagList CreateTags(
        string repository,
        string operation,
        string outcome,
        string? failureType = null)
    {
        var tags = new TagList
        {
            { "repository.name", repository },
            { "repository.operation", operation },
            { "outcome", outcome }
        };

        if (failureType is not null)
        {
            tags.Add("error.type", failureType);
        }

        return tags;
    }
}
