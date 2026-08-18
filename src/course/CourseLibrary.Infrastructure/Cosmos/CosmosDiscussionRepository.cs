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

    public Task<IReadOnlyList<Discussion>> GetByCourseAsync(string courseId, CancellationToken cancellationToken = default)
    {
        var query = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.updatedAt DESC")
            .WithParameter("@courseId", courseId);

        return _repository.QueryAsync(query, partitionKey: courseId, cancellationToken: cancellationToken);
    }

    public Task UpsertAsync(Discussion discussion, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(discussion, cancellationToken);

    public Task<bool> DeleteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(discussionId, courseId, cancellationToken);
}
