using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.Serialization.Observability;

internal sealed class SerializationMeter : MeterBase
{
    private static readonly Lazy<SerializationMeter> SharedInstance = new(
        static () => new SerializationMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static SerializationMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _operations;

    private SerializationMeter()
        : base(SerializationMeterConstants.MeterName)
    {
        _operations = CreateCounter<long>(
            SerializationMeterConstants.OperationsInstrumentName,
            "{operation}",
            "Number of serialization operations by serializer, operation, and outcome.");
    }

    public void RecordOperation(string serializer, string operation, string outcome)
    {
        var tags = new TagList
        {
            { SerializationMeterConstants.SerializerTag, serializer },
            { SerializationMeterConstants.OperationTag, operation },
            { SerializationMeterConstants.OutcomeTag, outcome }
        };

        _operations.Add(1, tags);
    }
}