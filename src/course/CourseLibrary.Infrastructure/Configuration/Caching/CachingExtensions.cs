using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Infrastructure.Configuration.Caching;

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
}