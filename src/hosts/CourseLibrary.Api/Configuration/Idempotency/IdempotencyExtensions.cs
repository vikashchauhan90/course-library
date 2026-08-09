using CourseLibrary.Application.Abstractions.Idempotency;
using CourseLibrary.Infrastructure.Idempotency;

namespace CourseLibrary.Api.Configuration.Idempotency;

internal static class IdempotencyExtensions
{
    public static IServiceCollection AddCourseLibraryIdempotency(this IServiceCollection services)
    {
        services.AddSingleton<IIdempotencyStore, CacheIdempotencyStore>();
        return services;
    }
}
