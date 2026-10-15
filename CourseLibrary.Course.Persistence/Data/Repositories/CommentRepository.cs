using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Platform.Persistence.CosmosDB;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using CourseLibrary.Platform.Persistence.CosmosDB.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Data.Repositories;

public sealed class CommentRepository
    : CosmosRepositoryBase<Comment, CommentId>,
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

    public Task<Comment?> GetByIdAsync(CommentId commentId, CourseId courseId, CancellationToken cancellationToken = default)
          => base.GetByIdAsync(commentId, courseId.Value, cancellationToken);

    public Task<IReadOnlyList<Comment>> GetByCourseAsync(CourseId courseId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.createdAt DESC")
            .WithParameter("@courseId", courseId);

        return base.QueryAsync(query, partitionKey: courseId.Value, cancellationToken: cancellationToken);
    }

    public Task<bool> DeleteAsync(CommentId commentId, CourseId courseId, CancellationToken cancellationToken = default)
            => base.DeleteAsync(commentId, courseId.Value, cancellationToken);

}
