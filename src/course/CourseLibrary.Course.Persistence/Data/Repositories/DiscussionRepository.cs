using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
using CourseLibrary.Course.Persistence.Observability;
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
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(DiscussionRepository),
            nameof(GetByIdAsync),
            () => base.GetByIdAsync(discussionId, [courseId.Value], cancellationToken));

    public Task<IReadOnlyList<Discussion>> GetByCourseAsync(CourseId courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(DiscussionRepository),
            nameof(GetByCourseAsync),
            () =>
            {
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.updatedAt DESC")
                    .WithParameter(
                        "@courseId",
                        Configuration.CreatePartitionKey(courseId.Value));

                return base.QueryAsync(
                    query,
                    partitionKey: [courseId.Value],
                    cancellationToken: cancellationToken);
            });

    public Task<bool> DeleteAsync(DiscussionId discussionId, CourseId courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(DiscussionRepository),
            nameof(DeleteAsync),
            () => base.DeleteAsync(discussionId, [courseId.Value], cancellationToken));

    public new Task UpsertAsync(Discussion discussion, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(DiscussionRepository),
            nameof(UpsertAsync),
            () => base.UpsertAsync(discussion, cancellationToken));
}
