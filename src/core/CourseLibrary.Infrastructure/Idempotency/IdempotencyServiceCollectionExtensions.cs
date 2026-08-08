using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Infrastructure.Idempotency;

public static class IdempotencyServiceCollectionExtensions
{
    public static IServiceCollection AddCourseLibraryIdempotency(
        this IServiceCollection services)
    {
        services.AddSingleton<IIdempotencyStore, CacheIdempotencyStore>();

        return services;
    }
}
