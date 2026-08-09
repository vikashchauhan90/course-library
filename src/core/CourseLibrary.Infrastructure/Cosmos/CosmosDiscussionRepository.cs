using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosDiscussionRepository : IDiscussionRepository
{
    private readonly ICosmosRepository<Discussion> _repository;

    public CosmosDiscussionRepository(ICosmosRepository<Discussion> repository)
    {
        _repository = repository;
    }

    public Task<Discussion?> GetByIdAsync(string discussionId, string courseId, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(discussionId, courseId, cancellationToken);

    public Task<IEnumerable<Discussion>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.updatedAt DESC";
        return _repository.QueryAsync(query, courseId, new Microsoft.Azure.Cosmos.QueryRequestOptions { PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(courseId) }, cancellationToken);
    }

    public Task UpsertAsync(Discussion discussion, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(discussion, cancellationToken);

    public Task DeleteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(discussionId, courseId, cancellationToken);
}
