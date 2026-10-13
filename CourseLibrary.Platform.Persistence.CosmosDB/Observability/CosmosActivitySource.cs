using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.Persistence.CosmosDB.Observability;

internal sealed class CosmosActivitySource : ActivitySourceBase
{
    public const string ActivitySourceName = "CourseLibrary.Platform.Persistence.CosmosDB";

    private static readonly Lazy<CosmosActivitySource> SharedInstance = new(
        static () => new CosmosActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CosmosActivitySource Shared => SharedInstance.Value;

    private CosmosActivitySource()
        : base(ActivitySourceName)
    {
    }
}
