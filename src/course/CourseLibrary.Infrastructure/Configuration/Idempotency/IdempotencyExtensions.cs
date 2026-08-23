using CourseLibrary.Application.Abstractions.Idempotency;
using CourseLibrary.Infrastructure.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Infrastructure.Configuration.Idempotency;

public static class IdempotencyExtensions
{
    public static IServiceCollection AddCourseLibraryIdempotency(this IServiceCollection services)
    {
        services.AddSingleton<IIdempotencyStore, CacheIdempotencyStore>();
        return services;
    }
}
