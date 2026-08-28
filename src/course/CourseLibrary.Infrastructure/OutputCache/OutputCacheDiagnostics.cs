
namespace CourseLibrary.Infrastructure.OutputCache;


public interface IOutputCacheDiagnostics
{
    public bool? Hit { get; set; }

    public TimeSpan? ExpirationDuration { get; set; }
}

public sealed class OutputCacheDiagnostics: IOutputCacheDiagnostics
{
    public bool? Hit { get; set; }
    public TimeSpan? ExpirationDuration { get; set; }
}