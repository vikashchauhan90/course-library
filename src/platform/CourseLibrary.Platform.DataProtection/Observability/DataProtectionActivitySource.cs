using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.DataProtection.Observability;

internal sealed class DataProtectionActivitySource : ActivitySourceBase
{
    private static readonly Lazy<DataProtectionActivitySource> SharedInstance = new(
        static () => new DataProtectionActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static DataProtectionActivitySource Shared => SharedInstance.Value;

    private DataProtectionActivitySource()
        : base(DataProtectionTelemetry.ActivitySourceName)
    {
    }
}
