using System.Diagnostics;
using System.Diagnostics.Metrics;
using CourseLibrary.Platform.Observability;

namespace CourseLibrary.Platform.Resilience.Distributed.Observability;

internal sealed class DistributedResilienceMeter : MeterBase
{
    private static readonly Lazy<DistributedResilienceMeter> SharedInstance = new(
        static () => new DistributedResilienceMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static DistributedResilienceMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _operations;

    private DistributedResilienceMeter()
        : base(DistributedResilienceTelemetry.MeterName)
    {
        _operations = CreateCounter<long>(
            DistributedResilienceMeterConstants.OperationsInstrumentName,
            "{operation}",
            "Number of distributed circuit coordination operations by operation and outcome.");
    }

    public void RecordOperation(string operation, string outcome)
    {
        var tags = new TagList
        {
            { DistributedResilienceMeterConstants.OperationTag, operation },
            { DistributedResilienceMeterConstants.OutcomeTag, outcome }
        };

        _operations.Add(1, tags);
    }
}
