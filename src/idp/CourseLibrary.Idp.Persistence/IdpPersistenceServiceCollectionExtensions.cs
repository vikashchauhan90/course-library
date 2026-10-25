using CourseLibrary.Idp.Persistence.Data;
using CourseLibrary.Idp.Persistence.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CourseLibrary.Idp.Persistence;

public static class IdpPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddIdpPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection must be configured.");
        }

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.FullName));
            options.EnableSensitiveDataLogging(isDevelopment);
            options.EnableDetailedErrors(isDevelopment);
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(serviceProvider.GetServices<IInterceptor>());
        }, optionsLifetime: ServiceLifetime.Scoped);

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
