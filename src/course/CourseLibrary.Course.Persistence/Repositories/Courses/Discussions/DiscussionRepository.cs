using CourseLibrary.Course.Persistence.Core;
using CourseLibrary.Course.Persistence.Cosmos;
using CourseLibrary.Course.Persistence.Entities;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Discussions;

internal sealed class DiscussionRepository 
    : CosmosContainer,
    IDiscussionRepository
{
    public DiscussionRepository(
       CosmosClient client,
       IOptions<CosmosOptions> options,
       ILogger<DiscussionRepository> logger)
       : base(client, options)
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
