using CourseLibrary.Api.Configuration.OutputCache;
using CourseLibrary.Infrastructure.Configuration.Caching;
using CourseLibrary.Infrastructure.Configuration.Cosmos;
using CourseLibrary.Infrastructure.Configuration.HttpContext;
using CourseLibrary.Infrastructure.Configuration.Idempotency;
using CourseLibrary.Infrastructure.Configuration.Messaging;
using CourseLibrary.Infrastructure.Configuration.Resilience;
using CourseLibrary.Infrastructure.Configuration.Serializers;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace CourseLibrary.Api.Configuration;

public static class InfrastructureServiceCollectionExtensions
{

    public static IServiceCollection AddCourseLibraryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCourseLibrarySerializers();
        services.AddCourseLibraryFusionCache(configuration);
        services.AddCourseLibraryIdempotency();
        services.AddCosmosDatabase(configuration);
        services.AddRepositories();
        services.AddCourseLibraryHttpResilience(configuration);
        services.AddCourseLibraryRequestContext();
        services.AddCourseLibraryServiceBus(configuration);
        services.AddCourseLibraryOutputCache();
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 64 * 1024 * 1024; // 64 MB
            options.SizeLimit = 100 * 1024 * 1024;      // 100 MB
            options.UseCaseSensitivePaths = true;
        });
        services.AddRequestTimeouts(options => {
            options.DefaultPolicy =
                new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromSeconds(60),
                    TimeoutStatusCode = 408,
                };
            options.AddPolicy("MyPolicy", TimeSpan.FromSeconds(2));
        });


        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.SmallestSize;
        });
        return services;
    }
}