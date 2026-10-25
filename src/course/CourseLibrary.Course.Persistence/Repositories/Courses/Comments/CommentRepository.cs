using CourseLibrary.Course.Persistence.Core;
using CourseLibrary.Course.Persistence.Cosmos;
using CourseLibrary.Course.Persistence.Entities;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Comments;

internal sealed class CommentRepository
    : CosmosContainer,
    ICommentRepository
{
    private readonly ILogger<CommentRepository> _logger;
    public CommentRepository(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ILogger<CommentRepository> logger)
        : base(client, options)
    {
        _logger = logger;
    }

    public Task<Comment?> GetByIdAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CommentRepository),
            nameof(GetByIdAsync),
            () => base.GetByIdAsync(commentId, CoursePartitionKeyFormats.ForCourse(courseId), cancellationToken));

    public Task<IReadOnlyList<Comment>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CommentRepository),
            nameof(GetByCourseAsync),
            () =>
            {
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.createdAt DESC")
                    .WithParameter("@courseId", courseId);

                return base.QueryAsync(
                    query,
                    partitionKey: CoursePartitionKeyFormats.ForCourse(courseId),
                    cancellationToken: cancellationToken);
            });

    public Task<bool> DeleteAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CommentRepository),
            nameof(DeleteAsync),
            () => base.DeleteAsync(commentId, CoursePartitionKeyFormats.ForCourse(courseId), cancellationToken));

    public new Task UpsertAsync(Comment comment, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CommentRepository),
            nameof(UpsertAsync),
            () => base.UpsertAsync(comment, cancellationToken));
}
