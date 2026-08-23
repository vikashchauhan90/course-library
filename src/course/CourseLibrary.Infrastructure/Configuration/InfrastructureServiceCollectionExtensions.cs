using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Infrastructure.Configuration.Caching;
using CourseLibrary.Infrastructure.Configuration.Cosmos;
using CourseLibrary.Infrastructure.Configuration.Idempotency;
using CourseLibrary.Infrastructure.Configuration.Messaging;
using CourseLibrary.Infrastructure.Configuration.Resilience;
using CourseLibrary.Infrastructure.Configuration.Serializers;
using CourseLibrary.Infrastructure.RequestContext;
using CourseLibrary.Infrastructure.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Infrastructure.Configuration;

public static class InfrastructureServiceCollectionExtensions
{

    public static IServiceCollection AddCourseLibraryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCourseLibrarySerializers();
        services.AddCourseLibraryMemoryCache();
        services.AddCourseLibraryIdempotency();
        services.AddCosmosDatabase(configuration);
        services.AddRepositories();
        services.AddCourseLibraryHttpResilience(configuration);
        services.AddScoped<IRequestContext, HttpRequestContext>();
        services.AddCourseLibraryServiceBus(configuration);

        return services;
    }
}
