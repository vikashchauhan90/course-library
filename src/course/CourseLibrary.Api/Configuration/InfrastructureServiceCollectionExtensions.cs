using CourseLibrary.Infrastructure.Configuration.Caching;
using CourseLibrary.Infrastructure.Configuration.Cosmos;
using CourseLibrary.Infrastructure.Configuration.Idempotency;
using CourseLibrary.Infrastructure.Configuration.Messaging;
using CourseLibrary.Infrastructure.Configuration.Resilience;
using CourseLibrary.Infrastructure.Configuration.Serializers;
using CourseLibrary.Infrastructure.Configuration.HttpContext;
using CourseLibrary.Api.Configuration.OutputCache;

namespace CourseLibrary.Api.Configuration;

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
        services.AddCourseLibraryRequestContext();
        services.AddCourseLibraryServiceBus(configuration);
        services.AddCourseLibraryOutputCache();
        return services;
    }
}