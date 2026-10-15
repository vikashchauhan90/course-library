using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.DataProtection.Observability;

internal sealed class DataProtectionMeter : MeterBase
{
    private static readonly Lazy<DataProtectionMeter> SharedInstance = new(
        static () => new DataProtectionMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static DataProtectionMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _operations;

    private DataProtectionMeter()
        : base(DataProtectionTelemetry.MeterName)
    {
        _operations = CreateCounter<long>(
            DataProtectionMeterConstants.OperationsInstrumentName,
            "{operation}",
            "Number of data protection operations by operation, payload type, and outcome.");
    }

    public void RecordOperation(string operation, string payloadType, string outcome)
    {
        var tags = new TagList
        {
            { DataProtectionMeterConstants.OperationTag, operation },
            { DataProtectionMeterConstants.PayloadTypeTag, payloadType },
            { DataProtectionMeterConstants.OutcomeTag, outcome }
        };

        _operations.Add(1, tags);
    }
}