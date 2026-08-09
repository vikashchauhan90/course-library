using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Infrastructure.Cosmos;

public static class CosmosRepositoryExtensions
{
    public static IServiceCollection AddCourseLibraryCosmosRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection("Cosmos").Get<CosmosOptions>()
            ?? throw new InvalidOperationException("Missing Cosmos configuration.");

        ValidateCosmosOptions(options);

        services.AddSingleton(options);
        services.AddSingleton(_ => CosmosClientFactory.Create(options));

        services.AddSingleton<ICosmosRepository<Course>>(sp =>
            new CosmosRepository<Course>(sp.GetRequiredService<CosmosClient>(), options.DatabaseName, options.CoursesContainer));

        services.AddSingleton<ICosmosRepository<Author>>(sp =>
            new CosmosRepository<Author>(sp.GetRequiredService<CosmosClient>(), options.DatabaseName, options.AuthorsContainer));

        services.AddSingleton<ICosmosRepository<Comment>>(sp =>
            new CosmosRepository<Comment>(sp.GetRequiredService<CosmosClient>(), options.DatabaseName, options.CommentsContainer));

        services.AddSingleton<ICosmosRepository<Discussion>>(sp =>
            new CosmosRepository<Discussion>(sp.GetRequiredService<CosmosClient>(), options.DatabaseName, options.DiscussionsContainer));

        services.AddSingleton<ICourseRepository>(sp =>
            new CosmosCourseRepository(sp.GetRequiredService<ICosmosRepository<Course>>()));

        services.AddSingleton<IAuthorRepository>(sp =>
            new CosmosAuthorRepository(sp.GetRequiredService<ICosmosRepository<Author>>()));

        services.AddSingleton<ICommentRepository>(sp =>
            new CosmosCommentRepository(sp.GetRequiredService<ICosmosRepository<Comment>>()));

        services.AddSingleton<IDiscussionRepository>(sp =>
            new CosmosDiscussionRepository(sp.GetRequiredService<ICosmosRepository<Discussion>>()));

        return services;
    }

    private static void ValidateCosmosOptions(CosmosOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AccountEndpoint) ||
            string.IsNullOrWhiteSpace(options.AccountKey) ||
            string.IsNullOrWhiteSpace(options.DatabaseName) ||
            string.IsNullOrWhiteSpace(options.CoursesContainer) ||
            string.IsNullOrWhiteSpace(options.AuthorsContainer) ||
            string.IsNullOrWhiteSpace(options.CommentsContainer) ||
            string.IsNullOrWhiteSpace(options.DiscussionsContainer))
        {
            throw new InvalidOperationException("Invalid Cosmos configuration. All container and account values are required.");
        }
    }
}
