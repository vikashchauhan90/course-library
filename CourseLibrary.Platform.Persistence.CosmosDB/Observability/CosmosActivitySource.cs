using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.Persistence.CosmosDB.Observability;

public sealed class CosmosActivitySource : ActivitySourceBase
{
    public const string DefaultSourceName = "CourseLibrary.Platform.Persistence.CosmosDB";

    public CosmosActivitySource()
        : base(DefaultSourceName)
    {
    }
}
