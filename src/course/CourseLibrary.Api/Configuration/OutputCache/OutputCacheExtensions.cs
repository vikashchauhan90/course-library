using CourseLibrary.Api.Configuration.OutputCache.Policies;
using CourseLibrary.Infrastructure.Caching;
using CourseLibrary.Infrastructure.OutputCache;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CourseLibrary.Api.Configuration.OutputCache;

public static class OutputCacheExtensions
{
    public static IServiceCollection AddCourseLibraryOutputCache(
        this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder =>
            {
                builder.Expire(TimeSpan.FromSeconds(30));
                builder.Tag("output-cache");             
            });

            options.AddPolicy(OutputCachePolicies.Default, policy =>
            {
                policy
                    .Expire(TimeSpan.FromMinutes(5))
                    .Tag("output-cache")
                    .AddPolicy<DefaultOutputCachePolicy>();
            });

            options.AddPolicy(
                OutputCachePolicies.Idempotency,
                policy =>
                {
                    policy
                    .Expire(TimeSpan.FromMinutes(5))
                    .SetVaryByHeader("Idempotency-Key")
                    .Tag("idempotency")
                    .AddPolicy<IdempotencyOutputCachePolicy>();
                });

            options.AddPolicy(OutputCachePolicies.NoLock, builder => builder.SetLocking(false));
            options.AddPolicy(OutputCachePolicies.NoCache, builder => builder.NoCache());
            options.AddPolicy(
                OutputCachePolicies.NoStore,
                policy => policy.AddPolicy<NoStoreOutputCachePolicy>());
        });
        
        services.Replace(
            ServiceDescriptor.Singleton<IOutputCacheStore, OutputCacheStore>());
        services.AddScoped<IOutputCacheDiagnostics, OutputCacheDiagnostics>();
        return services;
    }
}
