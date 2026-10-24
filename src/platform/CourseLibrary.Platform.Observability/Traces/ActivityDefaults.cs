using System.Diagnostics;

namespace CourseLibrary.Platform.Observability.Traces;

public static class ActivityDefaults
{
    public static void EnsureW3C()
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;
    }
}
