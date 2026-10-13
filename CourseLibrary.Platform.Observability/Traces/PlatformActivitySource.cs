
namespace CourseLibrary.Platform.Observability.Traces;

public sealed class PlatformActivitySource : ActivitySourceBase
{
    public const string DefaultSourceName = "CourseLibrary.Platform";

    private static readonly Lazy<PlatformActivitySource> SharedInstance = new(
        static () => new PlatformActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static PlatformActivitySource Shared => SharedInstance.Value;
    private PlatformActivitySource() : base(DefaultSourceName)
    {
    }
}
