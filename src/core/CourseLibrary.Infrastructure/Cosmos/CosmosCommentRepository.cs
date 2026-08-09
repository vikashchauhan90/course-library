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

    public Task<IEnumerable<Comment>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.createdAt DESC";
        return _repository.QueryAsync(query, courseId, new Microsoft.Azure.Cosmos.QueryRequestOptions { PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(courseId) }, cancellationToken);
    }

    public Task UpsertAsync(Comment comment, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(comment, cancellationToken);

    public Task DeleteAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(commentId, courseId, cancellationToken);
}
