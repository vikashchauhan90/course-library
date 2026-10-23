using CourseLibrary.Platform.Observability.Logs.Processors;
using CourseLibrary.Platform.Observability.Traces.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Platform.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservabilityServices(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IPrefixProvider, PlatformPrefixProvider>();
        services.AddSingleton<CourseLibraryLogProcessor>();
        services.AddSingleton<ActivityFailureProcessor>();
        services.AddSingleton<CorrelationActivityProcessor>();
        return services;
    }

}
