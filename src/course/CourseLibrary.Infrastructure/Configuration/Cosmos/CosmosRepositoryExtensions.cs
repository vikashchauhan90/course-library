using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Infrastructure.Cosmos;
using CourseLibrary.Infrastructure.Cosmos.Configurations;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos;

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
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy =
                        CosmosPropertyNamingPolicy.CamelCase
                },

                ApplicationName =
                    "CourseLibrary.Infrastructure.Cosmos"
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
        services.AddCosmosDocumentConfiguration<Course, CourseConfiguration>();
        services.AddCosmosDocumentConfiguration<CourseAuditEntry, CourseAuditEntryConfiguration>();
        services.AddCosmosDocumentConfiguration<Discussion, DiscussionConfiguration>();

        // Register repositories
        services.AddSingleton(typeof(ICosmosRepository<,>), typeof(CosmosRepository<,>));
        services.AddSingleton<ICommentRepository, CosmosCommentRepository>();
        services.AddSingleton<ICourseRepository, CosmosCourseRepository>();
        services.AddSingleton<ICourseAuditRepository, CosmosCourseAuditRepository>();
        services.AddSingleton<IDiscussionRepository, CosmosDiscussionRepository>();
        
    }

    public static void AddCosmosContainerInitializer(
        this IServiceCollection services)
    {
        services.AddHostedService<CosmosContainerInitializer>();
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
