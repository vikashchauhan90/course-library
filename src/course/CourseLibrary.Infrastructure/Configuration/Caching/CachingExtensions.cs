using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace CourseLibrary.Infrastructure.Configuration.Caching;

public static class CachingExtensions
{
    public static IServiceCollection AddCourseLibraryMemoryCache(
        this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddSingleton<ICacheProvider, MemoryCacheProvider>();

        return services;
    }

    public static IServiceCollection AddCourseLibraryRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var cacheOptions = configuration
           .GetSection(RedisOptions.SectionName)
           .Get<RedisOptions>()
           ?? throw new InvalidOperationException(
               "Redis configuration is missing.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
               cacheOptions.ConnectionString;

            options.InstanceName =
                cacheOptions.InstanceName;
        });

        services.AddSingleton<ICacheProvider, RedisCacheProvider>();

        return services;
    }

    public static IServiceCollection AddCourseLibraryHybridCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
           .AddOptions<RedisOptions>()
           .Bind(configuration.GetSection(RedisOptions.SectionName))
           .ValidateDataAnnotations()
           .ValidateOnStart();

        var cacheOptions = configuration
           .GetSection(RedisOptions.SectionName)
           .Get<RedisOptions>()
           ?? throw new InvalidOperationException(
               "Redis configuration is missing.");

        // L2 distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
               cacheOptions.ConnectionString;

            options.InstanceName =
                cacheOptions.InstanceName;
        });

        services.AddHybridCache();

        services.AddSingleton<ICacheProvider, HybridCacheProvider>();

        return services;
    }

    public static IServiceCollection AddCourseLibraryFusionCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
           .AddOptions<RedisOptions>()
           .Bind(configuration.GetSection(RedisOptions.SectionName))
           .ValidateDataAnnotations()
           .ValidateOnStart();

        var cacheOptions = configuration
           .GetSection(RedisOptions.SectionName)
           .Get<RedisOptions>()
           ?? throw new InvalidOperationException(
               "Redis configuration is missing.");

        // L2 distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
               cacheOptions.ConnectionString;

            options.InstanceName =
                cacheOptions.InstanceName;
        });

        // Redis backplane
        services.AddFusionCacheStackExchangeRedisBackplane(options =>
        {
            options.Configuration =
            cacheOptions.ConnectionString;
        });

        services.AddFusionCache()
            .WithDefaultEntryOptions(options =>
            {
                options.Duration = TimeSpan.FromMinutes(5);

                options.IsFailSafeEnabled = true;
                options.FailSafeMaxDuration = TimeSpan.FromHours(1);
                options.FailSafeThrottleDuration = TimeSpan.FromSeconds(30);

                options.FactorySoftTimeout =
                    TimeSpan.FromMilliseconds(500);

                options.FactoryHardTimeout =
                    TimeSpan.FromSeconds(5);
            })
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        services.AddSingleton<ICacheProvider, FusionCacheProvider>();

        return services;
    }
}