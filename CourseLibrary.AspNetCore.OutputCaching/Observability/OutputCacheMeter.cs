using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.AspNetCore.OutputCaching.Observability;

internal sealed class OutputCacheMeter : MeterBase
{
    private static readonly Lazy<OutputCacheMeter> SharedInstance = new(
        static () => new OutputCacheMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static OutputCacheMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _operations;

    private OutputCacheMeter()
        : base(OutputCacheTelemetry.MeterName)
    {
        _operations = CreateCounter<long>(
            OutputCacheMeterConstants.OperationsInstrumentName,
            "{operation}",
            "Number of output cache operations by operation and outcome.");
    }

    public void RecordOperation(string operation, string outcome)
    {
        var tags = new TagList
        {
            { OutputCacheMeterConstants.OperationTag, operation },
            { OutputCacheMeterConstants.OutcomeTag, outcome }
        };

        _operations.Add(1, tags);
    }
}