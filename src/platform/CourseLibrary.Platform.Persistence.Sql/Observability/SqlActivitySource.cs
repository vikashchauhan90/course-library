using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.Persistence.Sql.Observability;

public sealed class SqlActivitySource : ActivitySourceBase
{
    private static readonly Lazy<SqlActivitySource> SharedInstance = new(
        static () => new SqlActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static SqlActivitySource Shared => SharedInstance.Value;

    private SqlActivitySource()
        : base(SqlActivitySourceConstants.ActivitySourceName)
    {
    }
}
