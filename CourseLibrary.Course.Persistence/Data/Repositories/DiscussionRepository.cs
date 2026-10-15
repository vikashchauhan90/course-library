using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
using CourseLibrary.Platform.Persistence.CosmosDB;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using CourseLibrary.Platform.Persistence.CosmosDB.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Data.Repositories;

internal sealed class DiscussionRepository 
    : CosmosRepositoryBase<Discussion, DiscussionId>,
    IDiscussionRepository
{
    public DiscussionRepository(
       CosmosClient client,
       IOptions<CosmosOptions> options,
       ICosmosDocumentConfiguration<Discussion> configuration,
       ILogger<DiscussionRepository> logger)
       : base(client, options, configuration, logger)
    {
    }

    public Task<Discussion?> GetByIdAsync(DiscussionId discussionId, CourseId courseId, CancellationToken cancellationToken = default)
        => base.GetByIdAsync(discussionId, courseId.Value, cancellationToken);

    public Task<IReadOnlyList<Discussion>> GetByCourseAsync(CourseId courseId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.updatedAt DESC")
            .WithParameter("@courseId", courseId);

        return base.QueryAsync(query, partitionKey: courseId.Value, cancellationToken: cancellationToken);
    }

    public Task<bool> DeleteAsync(DiscussionId discussionId, CourseId courseId, CancellationToken cancellationToken = default)
        => base.DeleteAsync(discussionId, courseId.Value, cancellationToken);
}
