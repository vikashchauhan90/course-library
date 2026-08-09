using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Caching;

namespace CourseLibrary.Api.Configuration.Caching;

internal static class CachingExtensions
{
    public static IServiceCollection AddCourseLibraryMemoryCache(
        this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddKeyedSingleton<ICacheProvider, MemoryCacheProvider>(
            CacheType.Memory);

        return services;
    }

    public static IServiceCollection AddCourseLibraryRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Redis connection string is not configured.");
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "CourseLibrary:";
        });

        services.AddKeyedSingleton<ICacheProvider, RedisCacheProvider>(
            CacheType.Redis);

        return services;
    }

    public static IServiceCollection AddCourseLibraryHybridCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HybridCache uses memory as its L1 cache.
        // If Redis is your L2 provider, register it first.
        var connectionString =
            configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Redis connection string is not configured.");
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "CourseLibrary:";
        });

        services.AddHybridCache();

        services.AddKeyedSingleton<ICacheProvider, HybridCacheProvider>(
            CacheType.Hybrid);

        return services;
    }

    public static IServiceCollection AddCourseLibraryCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cacheType = configuration.GetValue<CacheType>(
            "Caching:CacheType");

        switch (cacheType)
        {
            case CacheType.Memory:
                services.AddCourseLibraryMemoryCache();
                break;

            case CacheType.Redis:
                services.AddCourseLibraryRedisCache(configuration);
                break;

            case CacheType.Hybrid:
                services.AddCourseLibraryHybridCache(configuration);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported cache type: {cacheType}");
        }

        services.AddDefaultCacheProvider(cacheType);

        return services;
    }

    private static IServiceCollection AddDefaultCacheProvider(
        this IServiceCollection services,
        CacheType cacheType)
    {
        services.AddSingleton<ICacheProvider>(serviceProvider =>
            serviceProvider.GetRequiredKeyedService<ICacheProvider>(
                cacheType));

        return services;
    }
}