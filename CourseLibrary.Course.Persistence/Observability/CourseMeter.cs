using CourseLibrary.Platform.Observability;

namespace CourseLibrary.Course.Persistence.Observability;

internal sealed class CourseMeter : MeterBase
{
    public const string MeterName = "CourseLibrary.Course.Persistence";
    private static readonly Lazy<CourseMeter> SharedInstance = new(
        static () => new CourseMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);
    public static CourseMeter Shared => SharedInstance.Value;
    private CourseMeter()
        : base(MeterName)
    {
    }
}
