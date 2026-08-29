using CourseLibrary.Infrastructure.Configuration.Caching;
using CourseLibrary.Infrastructure.Configuration.Cosmos;
using CourseLibrary.Infrastructure.Configuration.Idempotency;
using CourseLibrary.Infrastructure.Configuration.Resilience;
using CourseLibrary.Infrastructure.Configuration.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.EventConsumer.Configuration;

public static class InfrastructureServiceCollectionExtensions
{

    public static IServiceCollection AddCourseLibraryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCourseLibrarySerializers();
        services.AddCourseLibraryFusionCache(configuration);
        services.AddCourseLibraryIdempotency();
        services.AddCosmosDatabase(configuration);
        services.AddRepositories();
        services.AddCourseLibraryHttpResilience(configuration);

        return services;
    }
}