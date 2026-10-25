using CourseLibrary.Course.Persistence.Cosmos;
using CourseLibrary.Course.Persistence.Cosmos.Serialization;
using CourseLibrary.Course.Persistence.Repositories;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using CourseLibrary.Course.Persistence.Repositories.Courses.Audits;
using CourseLibrary.Course.Persistence.Repositories.Courses.Comments;
using CourseLibrary.Course.Persistence.Repositories.Courses.Discussions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence;

public static class CosmosRepositoryExtensions
{

    public static IServiceCollection AddCosmosDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<CosmosOptions>()
            .Bind(configuration.GetSection(CosmosOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<CosmosClient>(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<CosmosOptions>>()
                .Value;

            var clientOptions = new CosmosClientOptions
            {
                Serializer = new CourseLibraryCosmosSerializer(
                    CosmosJsonSerializerOptions.CreateOptions()),

                ApplicationName =
                    "CourseLibrary.Course.Cosmos",

                MaxRetryAttemptsOnRateLimitedRequests = 3,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
            };

            return new CosmosClient(
                options.AccountEndpoint,
                options.AccountKey,
                clientOptions);
        });

        return services;

    }

    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<
            ICosmosContainerFactory,
            CosmosContainerFactory>();

        // Register repositories
        services.AddSingleton<ICommentRepository, CommentRepository>();
        services.AddSingleton<ICourseRepository, CourseRepository>();
        services.AddSingleton<ICourseAuditRepository, CourseAuditRepository>();
        services.AddSingleton<IDiscussionRepository, DiscussionRepository>();

        return services;
    }
}
