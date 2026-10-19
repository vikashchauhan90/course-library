using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.Idempotency.Observability;

internal sealed class IdempotencyMeter : MeterBase
{
    private static readonly Lazy<IdempotencyMeter> SharedInstance = new(
        static () => new IdempotencyMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static IdempotencyMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _operations;
    private readonly Histogram<double> _duration;

    private IdempotencyMeter()
        : base(IdempotencyTelemetry.MeterName)
    {
        _operations = CreateCounter<long>(
            IdempotencyMeterConstants.OperationsInstrumentName,
            "{operation}",
            "Number of idempotency operations by operation and outcome.");
        _duration = CreateHistogram<double>(
            IdempotencyMeterConstants.DurationInstrumentName,
            "s",
            "Duration of idempotency operations.");
    }

    public void RecordOperation(string operation, string outcome, double durationSeconds)
    {
        var tags = new TagList
        {
            { IdempotencyMeterConstants.OperationTag, operation },
            { IdempotencyMeterConstants.OutcomeTag, outcome }
        };

        _operations.Add(1, tags);
        _duration.Record(durationSeconds, tags);
    }
}