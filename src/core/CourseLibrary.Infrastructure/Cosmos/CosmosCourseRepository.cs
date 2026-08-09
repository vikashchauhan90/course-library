using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosCourseRepository : ICourseRepository
{
    private readonly ICosmosRepository<Course> _repository;

    public CosmosCourseRepository(ICosmosRepository<Course> repository)
    {
        _repository = repository;
    }

    public Task<Course?> GetByIdAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(courseId, partitionKey, cancellationToken);

    public Task<IEnumerable<Course>> GetByAuthorAsync(string authorId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM c WHERE c.authorId = @authorId ORDER BY c.updatedAt DESC";
        return _repository.QueryAsync(query, authorId, new Microsoft.Azure.Cosmos.QueryRequestOptions { PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(authorId) }, cancellationToken);
    }

    public Task<IEnumerable<Course>> SearchAsync(string query, int pageSize, string? continuationToken, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT * FROM c WHERE CONTAINS(c.title, @query) OR CONTAINS(c.description, @query) ORDER BY c.updatedAt DESC";
        return _repository.QueryAsync(sql, string.Empty, new Microsoft.Azure.Cosmos.QueryRequestOptions { MaxItemCount = pageSize }, cancellationToken);
    }

    public Task UpsertAsync(Course course, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(course, cancellationToken);

    public Task DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(courseId, partitionKey, cancellationToken);
}
