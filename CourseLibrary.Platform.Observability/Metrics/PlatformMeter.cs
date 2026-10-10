
namespace CourseLibrary.Platform.Observability.Metrics;

public sealed class PlatformMeter : MeterBase
{
    public const string DefaultMeterName = "CourseLibrary.Platform";
    public PlatformMeter() : base(DefaultMeterName)
    {
    }
}
