using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Caching;

namespace CourseLibrary.Api.Configuration.Caching;

internal static class CachingExtensions
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

        services.AddSingleton<ICacheProvider, RedisCacheProvider>();

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

        services.AddSingleton<ICacheProvider, HybridCacheProvider>();

        return services;
    }
}