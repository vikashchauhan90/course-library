using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Infrastructure.Cosmos;
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

    public  static void AddRepositories(
        this IServiceCollection services)
    {
        services.AddSingleton(typeof(ICosmosRepository<>), typeof(CosmosRepository<>));
        services.AddSingleton<IAuthorRepository, CosmosAuthorRepository>();
        services.AddSingleton<ICommentRepository, CosmosCommentRepository>();
        services.AddSingleton<ICourseRepository, CosmosCourseRepository>();
        services.AddSingleton<IDiscussionRepository, CosmosDiscussionRepository>();
    }
}
