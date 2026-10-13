using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.Persistence.Observability;

public sealed class CosmosActivitySource : ActivitySourceBase
{
    public const string DefaultSourceName = "CourseLibrary.Course.Persistence.Cosmos";

    public CosmosActivitySource() : base(DefaultSourceName)
    {
    }
}
