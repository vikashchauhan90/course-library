using CourseLibrary.Platform.Caching.Abstractions;
using CourseLibrary.Platform.Caching.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.NeueccMessagePack;

namespace CourseLibrary.Platform.Caching;

public static class CacheExtensions
{

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
            new ConfigurationOptions
            {
                EndPoints = { cacheOptions.ConnectionString },
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                ConnectTimeout = 5000,
                SyncTimeout = 5000
            };
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(configuration.ConnectionString);
        });

        // Redis backplane
        services.AddFusionCacheStackExchangeRedisBackplane(options =>
        {
            options.Configuration =
            cacheOptions.ConnectionString;
            options.ConfigurationOptions =
            new ConfigurationOptions
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
