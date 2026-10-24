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

internal sealed class CommentRepository
    : CosmosRepositoryBase<Comment, string>,
    ICommentRepository
{
    public CommentRepository(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ICosmosDocumentConfiguration<Comment> configuration,
        ILogger<CommentRepository> logger)
        : base(client, options, configuration, logger)
    {
    }

    public Task<Comment?> GetByIdAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CommentRepository),
            nameof(GetByIdAsync),
            () => base.GetByIdAsync(commentId, [courseId], cancellationToken));

    public Task<IReadOnlyList<Comment>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CommentRepository),
            nameof(GetByCourseAsync),
            () =>
            {
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.createdAt DESC")
                    .WithParameter(
                        "@courseId",
                        Configuration.CreatePartitionKey(courseId));

                return base.QueryAsync(
                    query,
                    partitionKey: [courseId],
                    cancellationToken: cancellationToken);
            });

    public Task<bool> DeleteAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CommentRepository),
            nameof(DeleteAsync),
            () => base.DeleteAsync(commentId, [courseId], cancellationToken));

    public new Task UpsertAsync(Comment comment, CancellationToken cancellationToken = default)
        => CourseRepositoryObservability.ExecuteAsync(
            nameof(CommentRepository),
            nameof(UpsertAsync),
            () => base.UpsertAsync(comment, cancellationToken));
}
