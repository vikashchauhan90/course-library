using CourseLibrary.Api.Configuration.OutputCache.Policies;
using CourseLibrary.Infrastructure.Caching;
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
                    .Tag("output-cache");
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
        });

        services.Replace(
    ServiceDescriptor.Singleton<IOutputCacheStore, OutputCacheStore>());
        return services;
    }
}
