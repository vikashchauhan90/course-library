using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Models;

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

    public Task<PageResult<Course>> GetByAuthorAsync(
        string authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        var query = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c WHERE c.authorId = @authorId ORDER BY c.updatedAt DESC")
            .WithParameter("@authorId", authorId);

        return _repository.QueryPageAsync(
            query,
            partitionKey: authorId,
            continuationToken,
            pageSize,
            cancellationToken);
    }

    public Task<PageResult<Course>> SearchAsync(
        string query,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        var sql = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c WHERE CONTAINS(c.title, @query) OR CONTAINS(c.description, @query) ORDER BY c.updatedAt DESC")
            .WithParameter("@query", query);

        return _repository.QueryPageAsync(
            sql,
            continuationToken: continuationToken,
            pageSize: pageSize,
            cancellationToken: cancellationToken);
    }

    public Task UpsertAsync(Course course, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(course, cancellationToken);

    public Task<bool> DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(courseId, partitionKey, cancellationToken);
}
