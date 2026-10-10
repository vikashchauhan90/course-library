using CourseLibrary.Platform.Observability.Logs.Processors;
using CourseLibrary.Platform.Observability.Traces.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Platform.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddCourseLibraryObservabilityServices(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<CourseLibraryLogProcessor>();
        services.AddSingleton<ActivityFailureProcessor>();
        services.AddSingleton<CorrelationActivityProcessor>();
        return services;
    }

}
