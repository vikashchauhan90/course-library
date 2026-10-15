using CourseLibrary.Course.Persistence;
using CourseLibrary.Platform.Messaging;
using CourseLibrary.Platform.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Course.EventConsumer.Configuration;

public static class CourseLibraryEventConsumerServiceCollectionExtensions
{
    public static IServiceCollection AddCourseLibraryEventConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddCourseLibrarySerializers();
        services.AddCourseLibraryServiceBus(configuration);
        services.AddCosmosDatabase(configuration);
        services.AddRepositories();

        return services;
    }
}
