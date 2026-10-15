using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.Resilience.Observability;

internal sealed class ResilienceMeter : MeterBase
{
    private static readonly Lazy<ResilienceMeter> SharedInstance = new(
        static () => new ResilienceMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ResilienceMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _operations;

    private ResilienceMeter()
        : base(ResilienceTelemetry.MeterName)
    {
        _operations = CreateCounter<long>(
            ResilienceMeterConstants.OperationsInstrumentName,
            "{operation}",
            "Number of resilience pipeline executions by operation and outcome.");
    }

    public void RecordOperation(string operation, string outcome)
    {
        var tags = new TagList
        {
            { ResilienceMeterConstants.OperationTag, operation },
            { ResilienceMeterConstants.OutcomeTag, outcome }
        };

        _operations.Add(1, tags);
    }
}