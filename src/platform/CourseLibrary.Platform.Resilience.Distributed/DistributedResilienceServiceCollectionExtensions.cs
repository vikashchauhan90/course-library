using CourseLibrary.Platform.Resilience.Distributed.Abstractions;
using CourseLibrary.Platform.Resilience.Distributed.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CourseLibrary.Platform.Resilience.Distributed;

public static class DistributedResilienceServiceCollectionExtensions
{
    /// <summary>
    /// Registers the distributed resilience executor and Redis state store.
    /// An <see cref="StackExchange.Redis.IConnectionMultiplexer"/> must already be registered.
    /// </summary>
    public static IServiceCollection AddCourseLibraryDistributedResilience(
        this IServiceCollection services,
        Action<DistributedResilienceOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.AddOptions<DistributedResilienceOptions>()
            .Validate(
                options =>
                {
                    options.Validate();
                    return true;
                },
                "Distributed resilience options are invalid.")
            .ValidateOnStart();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        services.TryAddSingleton<IDistributedResilienceStore, RedisDistributedResilienceStore>();
        services.TryAddSingleton<IDistributedResilience, DistributedResilience>();

        return services;
    }
}
