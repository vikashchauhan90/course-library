using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Persistence.Data.Configurations;
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
    : CosmosRepositoryBase<Discussion, string>,
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

    public Task<Discussion?> GetByIdAsync(string discussionId, string courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(DiscussionRepository),
            nameof(GetByIdAsync),
            () => base.GetByIdAsync(discussionId, CoursePartitionKeyFormats.ForCourse(courseId), cancellationToken));

    public Task<IReadOnlyList<Discussion>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(DiscussionRepository),
            nameof(GetByCourseAsync),
            () =>
            {
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.updatedAt DESC")
                    .WithParameter("@courseId", courseId);

                return base.QueryAsync(
                    query,
                    partitionKey: CoursePartitionKeyFormats.ForCourse(courseId),
                    cancellationToken: cancellationToken);
            });

    public Task<bool> DeleteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(DiscussionRepository),
            nameof(DeleteAsync),
            () => base.DeleteAsync(discussionId, CoursePartitionKeyFormats.ForCourse(courseId), cancellationToken));

    public new Task UpsertAsync(Discussion discussion, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(DiscussionRepository),
            nameof(UpsertAsync),
            () => base.UpsertAsync(discussion, cancellationToken));
}
