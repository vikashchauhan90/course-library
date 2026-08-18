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

    public Task<IReadOnlyList<Course>> GetByAuthorAsync(string authorId, CancellationToken cancellationToken = default)
    {
        var query = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c WHERE c.authorId = @authorId ORDER BY c.updatedAt DESC")
            .WithParameter("@authorId", authorId);

        return _repository.QueryAsync(query, partitionKey: authorId, cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<Course>> SearchAsync(string query, int pageSize, string? continuationToken, CancellationToken cancellationToken = default)
    {
        var sql = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c WHERE CONTAINS(c.title, @query) OR CONTAINS(c.description, @query) ORDER BY c.updatedAt DESC")
            .WithParameter("@query", query);

        return _repository.QueryAsync(sql, cancellationToken: cancellationToken);
    }

    public Task UpsertAsync(Course course, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(course, cancellationToken);

    public Task<bool> DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(courseId, partitionKey, cancellationToken);
}
