using CourseLibrary.Platform.Observability.Logs.Processors;
using CourseLibrary.Platform.Observability.Traces.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CourseLibrary.Platform.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservabilityServices(
        this IServiceCollection services,
        Action<ObservabilityOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ObservabilityOptions();
        configure?.Invoke(options);
        services.TryAddSingleton(options);
        if (!string.IsNullOrWhiteSpace(options.LogKeyPrefix))
        {
            services.TryAddSingleton<IPrefixProvider>(
                new StaticPrefixProvider(options.LogKeyPrefix));
        }
        else
        {
            services.TryAddSingleton<IPrefixProvider, PlatformPrefixProvider>();
        }
        services.TryAddSingleton<CourseLibraryLogProcessor>();
        services.TryAddSingleton<ActivityFailureProcessor>();
        services.TryAddSingleton<CorrelationActivityProcessor>();
        return services;
    }

}
