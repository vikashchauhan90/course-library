using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosCommentRepository : ICommentRepository
{
    private readonly ICosmosRepository<Comment> _repository;

    public CosmosCommentRepository(ICosmosRepository<Comment> repository)
    {
        _repository = repository;
    }

    public Task<Comment?> GetByIdAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(commentId, courseId, cancellationToken);

    public Task<IReadOnlyList<Comment>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default)
    {
        var query = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.createdAt DESC")
            .WithParameter("@courseId", courseId);

        return _repository.QueryAsync(query, partitionKey: courseId, cancellationToken: cancellationToken);
    }

    public Task UpsertAsync(Comment comment, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(comment, cancellationToken);

    public Task DeleteAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(commentId, courseId, cancellationToken);
}
