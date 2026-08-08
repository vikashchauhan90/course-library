using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Infrastructure.Caching;

public static class CachingServiceCollectionExtensions
{
    public static IServiceCollection AddCourseLibraryCaching(
        this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheProvider, MemoryCacheProvider>();

        return services;
    }
}
