
namespace CourseLibrary.Platform.Observability.Traces;

public sealed class PlatformActivitySource : ActivitySourceBase
{
    public const string DefaultSourceName = "CourseLibrary.Platform";
    public PlatformActivitySource() : base(DefaultSourceName)
    {
    }
}
