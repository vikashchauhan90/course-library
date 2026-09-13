using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosCommentRepository : ICommentRepository
{
    private readonly ICosmosRepository<Comment, CommentId> _repository;

    public CosmosCommentRepository(ICosmosRepository<Comment, CommentId> repository)
    {
        _repository = repository;
    }

    public Task<Comment?> GetByIdAsync(CommentId commentId, CourseId courseId, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(commentId, courseId.ToString(), cancellationToken);

    public Task<IReadOnlyList<Comment>> GetByCourseAsync(CourseId courseId, CancellationToken cancellationToken = default)
    {
        var query = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.createdAt DESC")
            .WithParameter("@courseId", courseId);

        return _repository.QueryAsync(query, partitionKey: courseId.ToString(), cancellationToken: cancellationToken);
    }

    public Task UpsertAsync(Comment comment, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(comment, cancellationToken);

    public Task<bool> DeleteAsync(CommentId commentId, CourseId courseId, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(commentId, courseId.ToString(), cancellationToken);
}
