
namespace CourseLibrary.Platform.Observability.Metrics;

public sealed class PlatformMeter : MeterBase
{
    public const string MeterName = "CourseLibrary.Platform";

    private static readonly Lazy<PlatformMeter> SharedInstance = new(
        static () => new PlatformMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static PlatformMeter Shared => SharedInstance.Value;
    private PlatformMeter() : base(MeterName)
    {
    }
}
