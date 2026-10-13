using CourseLibrary.Course.Persistence.Data;
using CourseLibrary.Course.Persistence.Data.Configurations;
using CourseLibrary.Course.Persistence.Data.Serialization;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CourseLibrary.Course.Domain.Repositories;

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
                    "CourseLibrary.Infrastructure.Cosmos"
            };

            return new CosmosClient(
                options.AccountEndpoint,
                options.AccountKey,
                clientOptions);
        });

        services.AddSingleton<CosmosActivitySource>();

    }

    public static void AddRepositories(
        this IServiceCollection services)
    {
        // Register document configurations
        services.AddCosmosDocumentConfiguration<Comment, CommentConfiguration>();
        services.AddCosmosDocumentConfiguration<CourseLibrary.Course.Domain.Entities.Course, CourseConfiguration>();
        services.AddCosmosDocumentConfiguration<CourseAuditEntry, CourseAuditEntryConfiguration>();
        services.AddCosmosDocumentConfiguration<Discussion, DiscussionConfiguration>();

        // Register repositories
        services.AddSingleton(typeof(ICosmosRepository<,>), typeof(CosmosRepository<,>));
        services.AddSingleton<ICommentRepository, CosmosCommentRepository>();
        services.AddSingleton<ICourseRepository, CosmosCourseRepository>();
        services.AddSingleton<ICourseAuditRepository, CosmosCourseAuditRepository>();
        services.AddSingleton<IDiscussionRepository, CosmosDiscussionRepository>();
        
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
