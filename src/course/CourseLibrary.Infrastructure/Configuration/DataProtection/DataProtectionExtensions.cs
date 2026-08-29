using CourseLibrary.Infrastructure.Configuration.Caching;
using CourseLibrary.Infrastructure.DataProtection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CourseLibrary.Infrastructure.Configuration.DataProtection;

public static class DataProtectionExtensions
{
    private const string APPLICATION_NAME = "CourseLibrary";
    private const string REDIS_KEY_NAME = "CourseLibrary:DataProtection:Keys";
    public static IServiceCollection AddCourseLibraryDataProtection(
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

        var redis = ConnectionMultiplexer.Connect(cacheOptions.ConnectionString);

        services.AddDataProtection()
                .SetApplicationName(APPLICATION_NAME)
                .PersistKeysToStackExchangeRedis(
                    redis,
                    REDIS_KEY_NAME
                )
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        services.AddSingleton<IDataProtectionService,
            DataProtectionService>();
        return services;

    }
}
