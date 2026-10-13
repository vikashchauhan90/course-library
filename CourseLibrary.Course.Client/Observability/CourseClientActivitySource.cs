using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.Client.Observability;

public sealed class CourseClientActivitySource : ActivitySourceBase
{
    public CourseClientActivitySource() : base(CourseClientActivitySourceConstants.ActivitySourceName)
    {
    }
}