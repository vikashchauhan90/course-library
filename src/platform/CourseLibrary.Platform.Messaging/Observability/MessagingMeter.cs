using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.Messaging.Observability;

internal sealed class MessagingMeter : MeterBase
{
    private static readonly Lazy<MessagingMeter> SharedInstance = new(
        static () => new MessagingMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static MessagingMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _operations;
    private readonly Histogram<double> _duration;
    private readonly Histogram<long> _messageSize;

    private MessagingMeter()
        : base(MessagingTelemetry.MeterName)
    {
        _operations = CreateCounter<long>(
            MessagingMeterConstants.OperationsInstrumentName,
            "{operation}",
            "Number of messaging operations by operation and outcome.");
        _duration = CreateHistogram<double>(
            MessagingMeterConstants.DurationInstrumentName,
            "s",
            "Duration of messaging operations.");
        _messageSize = CreateHistogram<long>(
            MessagingMeterConstants.MessageSizeInstrumentName,
            "By",
            "Size of published messaging payloads.");
    }

    public void RecordOperation(
        string operation,
        string outcome,
        double durationSeconds)
    {
        var tags = new TagList
        {
            { MessagingMeterConstants.OperationTag, operation },
            { MessagingMeterConstants.OutcomeTag, outcome }
        };

        _operations.Add(1, tags);
        _duration.Record(durationSeconds, tags);
    }

    public void RecordMessageSize(long sizeBytes)
    {
        _messageSize.Record(sizeBytes);
    }
}
