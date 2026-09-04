using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.NeueccMessagePack;

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

            options.ConfigurationOptions =
           new StackExchange.Redis.ConfigurationOptions
           {
               AbortOnConnectFail = false,
               ConnectRetry = 3,
               ConnectTimeout = 5000,
               SyncTimeout = 5000
           };
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

            options.ConfigurationOptions =
           new StackExchange.Redis.ConfigurationOptions
           {
               AbortOnConnectFail = false,
               ConnectRetry = 3,
               ConnectTimeout = 5000,
               SyncTimeout = 5000
           };
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

        // L1 in-memory cache
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1000;
        });

        // L2 distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
               cacheOptions.ConnectionString;

            options.InstanceName =
                cacheOptions.InstanceName;

            options.ConfigurationOptions =
            new StackExchange.Redis.ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                ConnectTimeout = 5000,
                SyncTimeout = 5000
            };
        });

        // Redis backplane
        services.AddFusionCacheStackExchangeRedisBackplane(options =>
        {
            options.Configuration =
            cacheOptions.ConnectionString;
            options.ConfigurationOptions = 
            new StackExchange.Redis.ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                ConnectTimeout = 5000,
                SyncTimeout = 5000
            };
        });

        services.AddFusionCache()
            .WithSerializer(
        new FusionCacheNeueccMessagePackSerializer())
            .WithDefaultEntryOptions(options =>
            {
                // L2 Redis lifetime
                options.Duration = TimeSpan.FromMinutes(5);

                // Fail-safe
                options.IsFailSafeEnabled = false;
                options.FailSafeMaxDuration = TimeSpan.FromHours(1);
                options.FailSafeThrottleDuration = TimeSpan.FromSeconds(30);

                // Factory timeouts
                options.FactorySoftTimeout =
                    TimeSpan.FromMilliseconds(500);

                options.FactoryHardTimeout =
                    TimeSpan.FromSeconds(5);

                // Distributed/backplane operations
                options.AllowBackgroundBackplaneOperations = true;
                options.AllowBackgroundDistributedCacheOperations = true;

                // L1 memory-cache lifetime/capacity
                options.MemoryCacheDuration = TimeSpan.FromMinutes(1);
                options.Priority = CacheItemPriority.Low;

                // One size unit per entry.
                options.Size = 1;
            })
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        services.AddSingleton<ICacheProvider, FusionCacheProvider>();

        return services;
    }
}