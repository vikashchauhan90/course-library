using System.Diagnostics;
using System.Diagnostics.Metrics;
using CourseLibrary.Platform.Observability;

namespace CourseLibrary.Platform.Persistence.CosmosDB.Observability;

public sealed class CosmosMeter : MeterBase
{
    private static readonly Lazy<CosmosMeter> SharedInstance = new(
        static () => new CosmosMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CosmosMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _operations;
    private readonly Counter<long> _failures;
    private readonly Histogram<double> _duration;
    private readonly Histogram<double> _requestCharge;

    private CosmosMeter()
        : base(CosmosMetricConstants.MeterName)
    {
        _operations = CreateCounter<long>(
            CosmosMetricConstants.OperationsInstrumentName,
            "{operation}",
            "Number of Cosmos DB repository operations.");
        _failures = CreateCounter<long>(
            CosmosMetricConstants.FailuresInstrumentName,
            "{failure}",
            "Number of failed or cancelled Cosmos DB repository operations.");
        _duration = CreateHistogram<double>(
            CosmosMetricConstants.DurationInstrumentName,
            "ms",
            "Duration of Cosmos DB repository operations.");
        _requestCharge = CreateHistogram<double>(
            CosmosMetricConstants.RequestChargeInstrumentName,
            "RU",
            "Request units consumed by Cosmos DB repository operations.");
    }

    public void RecordOperation(
        string operation,
        string outcome,
        double durationMilliseconds,
        double? requestCharge = null)
    {
        var tags = CreateTags(operation, outcome);

        _operations.Add(1, tags);
        _duration.Record(durationMilliseconds, tags);

        if (requestCharge.HasValue)
        {
            _requestCharge.Record(
                requestCharge.Value,
                CreateTags(operation, outcome));
        }
    }

    public void RecordFailure(string operation, string failureType)
        => _failures.Add(
            1,
            CreateTags(
                operation,
                failureType == "cancelled" ? "cancelled" : "failure",
                failureType));

    private static TagList CreateTags(
        string operation,
        string outcome,
        string? failureType = null)
    {
        var tags = new TagList
        {
            { CosmosActivityTagConstants.DbSystem, "cosmosdb" },
            { CosmosActivityTagConstants.DbOperationName, operation },
            { "outcome", outcome }
        };

        if (failureType is not null)
        {
            tags.Add(CosmosActivityTagConstants.ErrorType, failureType);
        }

        return tags;
    }
}
