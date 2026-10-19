using CourseLibrary.Platform.Persistence.Sql.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CourseLibrary.Platform.Persistence.Sql;

public static class PersistenceSqlServiceCollectionExtensions
{
    public static IServiceCollection AddCourseLibrarySqlPersistence(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IInterceptor, AuditEntityInterceptor>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IInterceptor, ConnectionLoggingInterceptor>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IInterceptor, QueryTimingInterceptor>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IInterceptor, SecurityEntityInterceptor>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IInterceptor, TransactionLoggingInterceptor>());

        return services;
    }
}
