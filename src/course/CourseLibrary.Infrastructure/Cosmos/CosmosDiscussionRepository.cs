using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosDiscussionRepository : IDiscussionRepository
{
    private readonly ICosmosRepository<Discussion, DiscussionId> _repository;

    public CosmosDiscussionRepository(ICosmosRepository<Discussion, DiscussionId> repository)
    {
        _repository = repository;
    }

    public Task<Discussion?> GetByIdAsync(DiscussionId discussionId, CourseId courseId, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(discussionId, courseId.ToString(), cancellationToken);

    public Task<IReadOnlyList<Discussion>> GetByCourseAsync(CourseId courseId, CancellationToken cancellationToken = default)
    {
        var query = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c WHERE c.courseId = @courseId ORDER BY c.updatedAt DESC")
            .WithParameter("@courseId", courseId);

        return _repository.QueryAsync(query, partitionKey: courseId.ToString(), cancellationToken: cancellationToken);
    }

    public Task UpsertAsync(Discussion discussion, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(discussion, cancellationToken);

    public Task<bool> DeleteAsync(DiscussionId discussionId, CourseId courseId, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(discussionId, courseId.ToString(), cancellationToken);
}
