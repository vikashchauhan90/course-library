using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Persistence.Data.Configurations;
using CourseLibrary.Course.Persistence.Data.Configurations.Serialization;
using CourseLibrary.Course.Persistence.Data.Repositories;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
using CourseLibrary.Platform.Persistence.CosmosDB;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence;

public static class CosmosRepositoryExtensions
{

    public static void AddCosmosDatabase(
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
                    ValueObjectJsonConverters.CreateOptions()),

                ApplicationName =
                    "CourseLibrary.Course.Cosmos"
            };

            return new CosmosClient(
                options.AccountEndpoint,
                options.AccountKey,
                clientOptions);
        });

    }

    public static void AddRepositories(
        this IServiceCollection services)
    {
        // Register document configurations
        services.AddCosmosDocumentConfiguration<Comment, CommentConfiguration>();
        services.AddCosmosDocumentConfiguration<Domain.Entities.Course, CourseConfiguration>();
        services.AddCosmosDocumentConfiguration<CourseAuditEntry, CourseAuditEntryConfiguration>();
        services.AddCosmosDocumentConfiguration<Discussion, DiscussionConfiguration>();

        // Register repositories
        services.AddSingleton<ICommentRepository, CommentRepository>();
        services.AddSingleton<ICourseRepository, CourseRepository>();
        services.AddSingleton<ICourseAuditRepository, CourseAuditRepository>();
        services.AddSingleton<IDiscussionRepository, DiscussionRepository>();
        
    }
    public static IServiceCollection AddCosmosDocumentConfiguration<TDocument, TConfiguration>(
    this IServiceCollection services)
    where TDocument : class
    where TConfiguration :
        class,
        ICosmosDocumentConfiguration<TDocument>
    {
        services.AddSingleton<TConfiguration>();

        services.AddSingleton<
            ICosmosDocumentConfiguration<TDocument>>(sp =>
            sp.GetRequiredService<TConfiguration>());

        services.AddSingleton<ICosmosDocumentConfiguration>(sp =>
            sp.GetRequiredService<TConfiguration>());

        return services;
    }
}
